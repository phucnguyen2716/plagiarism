using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlagiarismApi.Models.DTOs;
using PlagiarismApi.Services;

namespace PlagiarismApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class GradingController : ControllerBase
    {
        private readonly ISubmissionStore _store;
        private readonly IGradingService _gradingService;
        private readonly IPlagiarismService _plagiarismService;

        public GradingController(ISubmissionStore store, IGradingService gradingService, IPlagiarismService plagiarismService)
        {
            _store = store;
            _gradingService = gradingService;
            _plagiarismService = plagiarismService;
        }

        [HttpPost("grade/{studentId}")]
        public async Task<ActionResult<GradeResponse>> GradeSingle(string studentId)
        {
            var submission = _store.Get(studentId);
            if (submission == null)
            {
                return NotFound(new { detail = $"Submission not found for student ID: {studentId}" });
            }

            try
            {
                var result = await _gradingService.GradeSubmissionAsync(submission.ExtractedText);
                var gradedAt = DateTime.UtcNow.ToString("O");

                submission.Score = result.Score;
                submission.DraftFeedback = result.DraftFeedback;
                submission.GradedAt = gradedAt;

                // Sync plagiarism data during grading
                var otherSubmissions = _store.GetAllSubmissionsExcept(studentId);
                var plagResult = _plagiarismService.CheckPlagiarism(submission.ExtractedText, otherSubmissions);
                submission.PlagiarismRiskScore = plagResult.maxOverlapPct;
                submission.PlagiarismMatchedKeywords = plagResult.topKeywords;
                submission.PlagiarismMostSimilarTo = plagResult.similarStudentIds;

                _store.Upsert(submission);

                return Ok(new GradeResponse
                {
                    StudentId = studentId,
                    Score = result.Score,
                    DraftFeedback = result.DraftFeedback,
                    GradedAt = gradedAt
                });
            }
            catch (Exception e)
            {
                return StatusCode(502, new { detail = $"Gemini API call failed: {e.Message}" });
            }
        }

        [HttpPost("grade-all")]
        public async Task<ActionResult<GradeAllResponse>> GradeAll()
        {
            var allSubs = _store.GetAll();
            var results = new List<GradeAllResult>();
            int gradedCount = 0;
            int failedCount = 0;

            foreach (var kvp in allSubs)
            {
                var studentId = kvp.Key;
                var submission = kvp.Value;

                if (submission.Score != null) continue;

                try
                {
                    var result = await _gradingService.GradeSubmissionAsync(submission.ExtractedText);
                    var gradedAt = DateTime.UtcNow.ToString("O");

                    submission.Score = result.Score;
                    submission.DraftFeedback = result.DraftFeedback;
                    submission.GradedAt = gradedAt;

                    // Sync plagiarism data
                    var otherSubmissions = _store.GetAllSubmissionsExcept(studentId);
                    var plagResult = _plagiarismService.CheckPlagiarism(submission.ExtractedText, otherSubmissions);
                    submission.PlagiarismRiskScore = plagResult.maxOverlapPct;
                    submission.PlagiarismMatchedKeywords = plagResult.topKeywords;
                    submission.PlagiarismMostSimilarTo = plagResult.similarStudentIds;

                    _store.Upsert(submission);

                    results.Add(new GradeAllResult
                    {
                        StudentId = studentId,
                        Score = result.Score,
                        Success = true
                    });
                    gradedCount++;
                }
                catch (Exception e)
                {
                    results.Add(new GradeAllResult
                    {
                        StudentId = studentId,
                        Success = false,
                        Error = e.Message
                    });
                    failedCount++;
                }
            }

            return Ok(new GradeAllResponse
            {
                GradedCount = gradedCount,
                FailedCount = failedCount,
                Results = results
            });
        }
    }
}
