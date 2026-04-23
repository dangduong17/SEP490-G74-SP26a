using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RJMS.Vn.Edu.Fpt.Service
{
    /// <summary>
    /// Service gọi Gemini 1.5 Flash API để chấm điểm CV theo batch.
    /// Sử dụng SemaphoreSlim để tránh vượt rate limit Google API.
    /// </summary>
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;
        private readonly string _model;

        // Global throttle: chỉ 1 request Gemini tại một thời điểm để tránh lỗi 429
        private static readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1);

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GeminiAI:ApiKey"] ?? throw new InvalidOperationException("GeminiAI:ApiKey chưa được cấu hình.");
            _model = configuration["GeminiAI:Model"] ?? "gemini-1.5-flash";
        }

        public async Task<List<AiScoringResultDto>> ScoreCvBatchAsync(JobContextDto jobContext, List<CvBatchItemDto> cvBatch)
        {
            if (cvBatch == null || cvBatch.Count == 0)
                return new List<AiScoringResultDto>();

            var prompt = BuildBatchPrompt(jobContext, cvBatch);

            // Chờ semaphore để tránh rate limit 429
            await _rateLimitSemaphore.WaitAsync();
            try
            {
                return await CallGeminiAsync(prompt, cvBatch);
            }
            finally
            {
                // Delay nhỏ sau mỗi request để tránh burst
                await Task.Delay(500);
                _rateLimitSemaphore.Release();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE: Build Prompt
        // ─────────────────────────────────────────────────────────────────────
        private static string BuildBatchPrompt(JobContextDto job, List<CvBatchItemDto> cvBatch)
        {
            var sb = new StringBuilder();

            sb.AppendLine("You are an expert HR recruiter. Score CVs for the following job position.");
            sb.AppendLine();
            sb.AppendLine("=== JOB INFORMATION ===");
            sb.AppendLine($"Position: {job.Title}");

            if (!string.IsNullOrWhiteSpace(job.Requirements))
            {
                sb.AppendLine($"Requirements: {job.Requirements}");
            }
            if (!string.IsNullOrWhiteSpace(job.Description))
            {
                sb.AppendLine($"Description: {job.Description}");
            }
            if (job.RequiredSkills.Count > 0)
            {
                sb.AppendLine($"Required Skills: {string.Join(", ", job.RequiredSkills)}");
            }
            if (job.AllSkills.Count > 0)
            {
                sb.AppendLine($"All Skills (required + preferred): {string.Join(", ", job.AllSkills)}");
            }

            sb.AppendLine();
            sb.AppendLine("=== SCORING RULES ===");
            sb.AppendLine("- Score each CV from 0 to 10 (decimal allowed, e.g. 8.5)");
            sb.AppendLine("- Be strict and objective");
            sb.AppendLine("- Penalize missing required skills heavily");
            sb.AppendLine("- Reward measurable achievements and relevant experience");
            sb.AppendLine("- Consider: relevance, skills_match, experience, education, achievements, stability");
            sb.AppendLine();
            sb.AppendLine("=== CVs TO SCORE ===");

            foreach (var cv in cvBatch)
            {
                sb.AppendLine($"--- CV for application id={cv.ApplicationId} ---");
                sb.AppendLine(cv.JsonData);
                sb.AppendLine();
            }

            sb.AppendLine("=== OUTPUT FORMAT ===");
            sb.AppendLine("Return ONLY a valid JSON array. No explanation outside JSON. Each element:");
            sb.AppendLine(@"[
  {
    ""id"": <applicationId as integer>,
    ""aiScore"": <score 0-10 as decimal>,
    ""matchedSkills"": [""skill1"", ""skill2""],
    ""missingSkills"": [""skill3""],
    ""summary"": ""Brief assessment in Vietnamese (2-3 sentences)""
  }
]");
            sb.AppendLine("Do not explain outside JSON.");

            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE: Call Gemini REST API
        // ─────────────────────────────────────────────────────────────────────
        private async Task<List<AiScoringResultDto>> CallGeminiAsync(string prompt, List<CvBatchItemDto> cvBatch)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1,       // Thấp = ổn định, ít sáng tạo
                    maxOutputTokens = 4096,
                    responseMimeType = "application/json"
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Gemini API");
                throw new Exception($"Không thể kết nối Gemini API: {ex.Message}", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error {StatusCode}: {Body}", response.StatusCode, errorBody);
                throw new Exception($"Gemini API trả về lỗi {(int)response.StatusCode}: {errorBody[..Math.Min(500, errorBody.Length)]}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return ParseGeminiResponse(responseJson, cvBatch);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE: Parse Gemini Response
        // ─────────────────────────────────────────────────────────────────────
        private List<AiScoringResultDto> ParseGeminiResponse(string responseJson, List<CvBatchItemDto> cvBatch)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseJson);

                // Extract text from candidates[0].content.parts[0].text
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "[]";

                // Clean up markdown code blocks if Gemini wraps JSON
                text = text.Trim();
                if (text.StartsWith("```json")) text = text[7..];
                if (text.StartsWith("```")) text = text[3..];
                if (text.EndsWith("```")) text = text[..^3];
                text = text.Trim();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var results = JsonSerializer.Deserialize<List<AiScoringResultDto>>(text, options);
                if (results == null || results.Count == 0)
                {
                    throw new Exception("Gemini trả về JSON rỗng hoặc không hợp lệ.");
                }

                // Validate score range
                foreach (var r in results)
                {
                    r.AiScore = Math.Clamp(r.AiScore, 0m, 10m);
                    r.AiScore = Math.Round(r.AiScore, 1);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi parse Gemini response: {Response}", responseJson[..Math.Min(1000, responseJson.Length)]);
                throw new Exception($"Không thể parse kết quả từ Gemini: {ex.Message}", ex);
            }
        }
    }
}
