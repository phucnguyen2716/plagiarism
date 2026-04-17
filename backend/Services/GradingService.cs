using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PlagiarismApi.Services
{
    public interface IGradingService
    {
        Task<GradingResult> GradeSubmissionAsync(string text);
    }

    public class GradingResult
    {
        public int Score { get; set; }
        public string DraftFeedback { get; set; } = string.Empty;
    }

    public class GradingService : IGradingService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly string _model = "gemini-3-flash-preview"; 

        private const string MasterRubric = @"You are a university professor evaluating a student's Capstone Project / Thesis (Đồ án tốt nghiệp / Đồ án môn học) submission. Your task is to grade the submission against the following rubric. Be fair, consistent, and constructive.

## Grading Rubric (Total: 100 points)

### 1. Introduction and Problem Statement (0-20 points)
Award points based on:
- Clear definition of the problem being solved (up to 8 points)
- Motivation, objectives, and scope of the project (up to 7 points)
- Review of related work or existing solutions (up to 5 points)

### 2. System Design and Methodology (0-30 points)
Award points based on:
- Clear architectural design and system modeling (e.g., Use Case, UML, System Architecture) (up to 12 points)
- Justification of technology stack and tools selected (up to 8 points)
- Logic of the proposed algorithms, methodologies, or workflows (up to 10 points)

### 3. Implementation and Results (0-30 points)
Award points based on:
- Evidence of a working implementation (core features functioning) (up to 12 points)
- Testing methodology and quality assurance (up to 8 points)
- Clear presentation of results, metrics, or evaluation of the system (up to 10 points)

### 4. Documentation and Conclusion (0-20 points)
Award points based on:
- Formatting, clarity, and professional tone of the report (up to 10 points)
- Summary of achievements and limitations (up to 5 points)
- Realistic suggestions for future work/improvements (up to 5 points)

## Instructions
1. Read the student submission carefully.
2. Assign a total score out of 100 by summing your assessment across all four topic areas.
3. Write constructive, specific draft feedback (3-5 sentences) that identifies strengths and areas for improvement based on the criteria.
4. Your tone should be encouraging but academic. Mention specific sections the student excelled at and specific sections they should improve.
5. You MUST respond with valid JSON only, in exactly this format: {""score"": <number>, ""draft_feedback"": ""<string>""}
6. Do not include any text outside the JSON object.";

        public GradingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GEMINI_API_KEY"];
            
            if (string.IsNullOrEmpty(_apiKey))
            {
                // Note: The hardcoded key has been removed for security. 
                // Please ensure GEMINI_API_KEY is set in your .env file or environment variables.
                throw new InvalidOperationException("GEMINI_API_KEY is not configured. Please add it to your .env file.");
            }

            var modelOverride = configuration["GEMINI_MODEL"];
            if (!string.IsNullOrEmpty(modelOverride)) _model = modelOverride;
        }

        public async Task<GradingResult> GradeSubmissionAsync(string text)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("GEMINI_API_KEY is not configured.");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"Grade the following student submission:\n\n{text}" }
                        }
                    }
                },
                system_instruction = new
                {
                    parts = new[]
                    {
                        new { text = MasterRubric }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json",
                    temperature = 0.3
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API call failed with status {response.StatusCode}: {errorBody}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            
            // Navigate to candidates[0].content.parts[0].text
            var resultText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(resultText))
                throw new Exception("Empty response from Gemini API.");

            var result = JsonSerializer.Deserialize<GradingResult>(resultText, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true 
            });
            
            if (result == null) throw new Exception("Failed to parse grading result.");

            result.Score = Math.Clamp(result.Score, 0, 100);
            return result;
        }
    }
}
