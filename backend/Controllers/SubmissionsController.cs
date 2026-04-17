using Microsoft.AspNetCore.Mvc;
using PlagiarismApi.Models.DTOs;
using PlagiarismApi.Services;
using System.Linq;

namespace PlagiarismApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionStore _store;

        public SubmissionsController(ISubmissionStore store)
        {
            _store = store;
        }

        [HttpGet]
        public ActionResult<SubmissionListResponse> ListSubmissions()
        {
            var allSubs = _store.GetAll();
            var submissionsOut = allSubs.Values.Select(sub => new SubmissionOut
            {
                StudentId = sub.StudentId,
                Filename = sub.Filename,
                UploadedAt = sub.UploadedAt,
                Score = sub.Score,
                DraftFeedback = sub.DraftFeedback,
                PlagiarismRiskScore = sub.PlagiarismRiskScore,
                PlagiarismFlagged = sub.PlagiarismFlagged,
                PlagiarismMatchedKeywords = sub.PlagiarismMatchedKeywords,
                PlagiarismMostSimilarTo = sub.PlagiarismMostSimilarTo,
                GradedAt = sub.GradedAt
            }).ToList();

            var ungradedCount = allSubs.Values.Count(sub => sub.Score == null);

            return Ok(new SubmissionListResponse
            {
                Submissions = submissionsOut,
                Total = submissionsOut.Count,
                UngradedCount = ungradedCount
            });
        }
    }
}
