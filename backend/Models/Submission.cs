using System;
using System.Collections.Generic;

namespace PlagiarismApi.Models
{
    public class Submission
    {
        public string StudentId { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string ExtractedText { get; set; } = string.Empty;
        public int? Score { get; set; }
        public string? DraftFeedback { get; set; }
        public double PlagiarismRiskScore { get; set; }
        public bool PlagiarismFlagged { get; set; }
        public List<string> PlagiarismMatchedKeywords { get; set; } = new List<string>();
        public List<string> PlagiarismMostSimilarTo { get; set; } = new List<string>();
        public string UploadedAt { get; set; } = string.Empty;
        public string? GradedAt { get; set; }
    }
}
