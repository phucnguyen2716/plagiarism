using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlagiarismApi.Services
{
    public interface IPlagiarismService
    {
        (double maxOverlapPct, List<string> topKeywords, List<string> similarStudentIds) CheckPlagiarism(
            string newText, IDictionary<string, string> otherSubmissions);
    }

    public class PlagiarismService : IPlagiarismService
    {
        private const double K1 = 1.5;
        private const double B = 0.75;

        private static readonly HashSet<string> Stopwords = new()
        {
            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
            "of", "with", "by", "from", "is", "are", "was", "were", "be", "been",
            "being", "have", "has", "had", "do", "does", "did", "will", "would",
            "could", "should", "may", "might", "shall", "can", "need", "must",
            "it", "its", "this", "that", "these", "those", "i", "you", "he", "she",
            "we", "they", "me", "him", "her", "us", "them", "my", "your", "his",
            "our", "their", "what", "which", "who", "whom", "how", "when", "where",
            "why", "not", "no", "nor", "as", "if", "then", "than", "so", "such",
            "also", "each", "every", "all", "any", "few", "more", "most", "other",
            "some", "about", "up", "out", "into", "over", "after", "before"
        };

        private List<string> Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            text = text.ToLower();
            text = Regex.Replace(text, @"[^\w\s]", "");
            var tokens = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Where(t => !Stopwords.Contains(t) && t.Length > 1).ToList();
        }

        public (double maxOverlapPct, List<string> topKeywords, List<string> similarStudentIds) CheckPlagiarism(
            string newText, IDictionary<string, string> otherSubmissions)
        {
            if (otherSubmissions == null || otherSubmissions.Count == 0)
                return (0.0, new List<string>(), new List<string>());

            var queryTokens = Tokenize(newText);
            if (queryTokens.Count == 0)
                return (0.0, new List<string>(), new List<string>());

            var queryCounts = queryTokens.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
            
            // Build Corpus Stats
            var corpus = new Dictionary<string, List<string>>();
            foreach (var kvp in otherSubmissions)
            {
                corpus[kvp.Key] = Tokenize(kvp.Value);
            }

            int N = corpus.Count + 1; // Include query doc in stats
            double avgdl = (corpus.Values.Sum(v => v.Count) + queryTokens.Count) / (double)N;

            // Document Frequency
            var df = new Dictionary<string, int>();
            var allDocs = corpus.Values.Concat(new[] { queryTokens });
            foreach (var doc in allDocs)
            {
                foreach (var term in doc.Distinct())
                {
                    df[term] = df.GetValueOrDefault(term) + 1;
                }
            }

            // Calculation
            double maxScore = 0.0;
            var results = new List<(string studentId, double score, List<string> keywords)>();

            // Reference score for normalization (Query compared to itself)
            double selfScore = CalculateBM25(queryTokens, queryTokens, df, N, avgdl, out _);

            foreach (var kvp in corpus)
            {
                double score = CalculateBM25(queryTokens, kvp.Value, df, N, avgdl, out var sharedKeywords);
                
                // Convert to percentage relative to self-match
                double riskPct = (selfScore > 0) ? (score / selfScore) * 100.0 : 0.0;
                riskPct = Math.Min(100.0, Math.Round(riskPct, 1));

                if (riskPct > 0)
                {
                    results.Add((kvp.Key, riskPct, sharedKeywords));
                }
            }

            var sortedResults = results.OrderByDescending(r => r.score).ToList();
            var topResult = sortedResults.FirstOrDefault();

            return (
                topResult.score,
                topResult.keywords ?? new List<string>(),
                sortedResults.Select(r => r.studentId).ToList()
            );
        }

        private double CalculateBM25(List<string> query, List<string> doc, Dictionary<string, int> df, int N, double avgdl, out List<string> topKeywords)
        {
            var docCounts = doc.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
            double score = 0.0;
            var termScores = new Dictionary<string, double>();

            foreach (var term in query.Distinct())
            {
                if (docCounts.TryGetValue(term, out int fq))
                {
                    double idf = Math.Log((N - df[term] + 0.5) / (df[term] + 0.5) + 1.0);
                    double num = fq * (K1 + 1);
                    double den = fq + K1 * (1 - B + B * (doc.Count / avgdl));
                    double termScore = idf * (num / den);
                    score += termScore;
                    termScores[term] = termScore;
                }
            }

            topKeywords = termScores.OrderByDescending(x => x.Value).Take(15).Select(x => x.Key).ToList();
            return score;
        }
    }
}
