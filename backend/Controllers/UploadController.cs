using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlagiarismApi.Models;
using PlagiarismApi.Models.DTOs;
using PlagiarismApi.Services;

namespace PlagiarismApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ISubmissionStore _store;
        private readonly IPdfService _pdfService;
        private readonly IPlagiarismService _plagiarismService;
        private static readonly Regex StudentIdPattern = new(@"^(S\d+)[_\-]", RegexOptions.Compiled);

        public UploadController(ISubmissionStore store, IPdfService pdfService, IPlagiarismService plagiarismService)
        {
            _store = store;
            _pdfService = pdfService;
            _plagiarismService = plagiarismService;
        }

        [HttpPost]
        public async Task<ActionResult<UploadResponse>> UploadSubmission(IFormFile file)
        {
            if (file == null || !file.FileName.ToLower().EndsWith(".pdf"))
            {
                return BadRequest(new { detail = "Only PDF files are accepted." });
            }

            var match = StudentIdPattern.Match(file.FileName);
            if (!match.Success)
            {
                return BadRequest(new { detail = "Could not extract student ID from filename. Expected format: S<digits>_<name>.pdf" });
            }

            var studentId = match.Groups[1].Value;
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var savePath = Path.Combine(uploadsDir, file.FileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractedText = _pdfService.ExtractText(savePath);
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return BadRequest(new { detail = "Could not extract any text from the PDF. The file may be image-based or empty." });
            }

            var otherSubmissions = _store.GetAllSubmissionsExcept(studentId);
            var (plagiarismRiskScore, matchedKeywords, mostSimilarTo) = _plagiarismService.CheckPlagiarism(extractedText, otherSubmissions);
            var plagiarismFlagged = plagiarismRiskScore >= 50.0;

            var submission = new Submission
            {
                StudentId = studentId,
                Filename = file.FileName,
                ExtractedText = extractedText,
                PlagiarismRiskScore = Math.Round(plagiarismRiskScore, 1),
                PlagiarismFlagged = plagiarismFlagged,
                PlagiarismMatchedKeywords = matchedKeywords,
                PlagiarismMostSimilarTo = mostSimilarTo,
                UploadedAt = DateTime.UtcNow.ToString("O")
            };

            _store.Upsert(submission);

            return Ok(new UploadResponse
            {
                StudentId = studentId,
                Filename = file.FileName,
                PlagiarismRiskScore = Math.Round(plagiarismRiskScore, 1),
                PlagiarismFlagged = plagiarismFlagged,
                Message = "Submission uploaded successfully"
            });
        }
    }
}
