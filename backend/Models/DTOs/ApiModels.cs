using System.Collections.Generic;

namespace PlagiarismApi.Models.DTOs
{
    public class UploadResponse
    {
        public string StudentId { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public double PlagiarismRiskScore { get; set; }
        public bool PlagiarismFlagged { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class GradeResponse
    {
        public string StudentId { get; set; } = string.Empty;
        public int Score { get; set; }
        public string DraftFeedback { get; set; } = string.Empty;
        public string GradedAt { get; set; } = string.Empty;
    }

    public class GradeAllResult
    {
        public string StudentId { get; set; } = string.Empty;
        public int? Score { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    public class GradeAllResponse
    {
        public int GradedCount { get; set; }
        public int FailedCount { get; set; }
        public List<GradeAllResult> Results { get; set; } = new List<GradeAllResult>();
    }

    public class SubmissionOut
    {
        public string StudentId { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string UploadedAt { get; set; } = string.Empty;
        public int? Score { get; set; }
        public string? DraftFeedback { get; set; }
        public double PlagiarismRiskScore { get; set; }
        public bool PlagiarismFlagged { get; set; }
        public List<string> PlagiarismMatchedKeywords { get; set; } = new List<string>();
        public List<string> PlagiarismMostSimilarTo { get; set; } = new List<string>();
        public string? GradedAt { get; set; }
    }

    public class SubmissionListResponse
    {
        public List<SubmissionOut> Submissions { get; set; } = new List<SubmissionOut>();
        public int Total { get; set; }
        public int UngradedCount { get; set; }
    }
}
