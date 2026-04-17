using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PlagiarismApi.Models;

namespace PlagiarismApi.Services
{
    public interface ISubmissionStore
    {
        Submission? Get(string studentId);
        IDictionary<string, Submission> GetAll();
        void Upsert(Submission submission);
        List<string> GetAllTextsExcept(string excludeId);
        IDictionary<string, string> GetAllSubmissionsExcept(string excludeId);
    }

    public class SubmissionStore : ISubmissionStore
    {
        private readonly string _filePath;
        private readonly ConcurrentDictionary<string, Submission> _store = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public SubmissionStore(IConfiguration configuration)
        {
            var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            
            _filePath = Path.Combine(dataDir, "submissions.json");
            Load();
        }

        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                try
                {
                    var data = JsonSerializer.Deserialize<Dictionary<string, Submission>>(json, _jsonOptions);
                    if (data != null)
                    {
                        foreach (var kvp in data)
                        {
                            _store[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch
                {
                    // Fallback or log error
                }
            }
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_store, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        public Submission? Get(string studentId) => _store.TryGetValue(studentId, out var sub) ? sub : null;

        public IDictionary<string, Submission> GetAll() => _store;

        public void Upsert(Submission submission)
        {
            _store[submission.StudentId] = submission;
            Save();
        }

        public List<string> GetAllTextsExcept(string excludeId)
        {
            return _store.Where(kvp => kvp.Key != excludeId && !string.IsNullOrEmpty(kvp.Value.ExtractedText))
                         .Select(kvp => kvp.Value.ExtractedText)
                         .ToList();
        }

        public IDictionary<string, string> GetAllSubmissionsExcept(string excludeId)
        {
            return _store.Where(kvp => kvp.Key != excludeId && !string.IsNullOrEmpty(kvp.Value.ExtractedText))
                         .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ExtractedText);
        }
    }
}
