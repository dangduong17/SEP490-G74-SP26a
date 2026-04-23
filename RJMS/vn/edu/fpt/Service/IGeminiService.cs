using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    /// <summary>
    /// Interface gọi Gemini AI để chấm điểm CV theo batch.
    /// </summary>
    public interface IGeminiService
    {
        /// <summary>
        /// Chấm điểm một batch (tối đa 5) CV cùng lúc.
        /// </summary>
        /// <param name="jobContext">Thông tin job (title, requirements, skills)</param>
        /// <param name="cvBatch">Danh sách tối đa 5 CV với applicationId và jsonData</param>
        /// <returns>Danh sách kết quả chấm điểm, mỗi phần tử tương ứng một CV</returns>
        Task<List<AiScoringResultDto>> ScoreCvBatchAsync(JobContextDto jobContext, List<CvBatchItemDto> cvBatch);
    }

    /// <summary>Thông tin context job để Gemini hiểu yêu cầu tuyển dụng.</summary>
    public class JobContextDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public string? Description { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> AllSkills { get; set; } = new();
    }

    /// <summary>Một CV cần chấm điểm, kèm applicationId để Gemini trả về đúng ID.</summary>
    public class CvBatchItemDto
    {
        public int ApplicationId { get; set; }
        public string JsonData { get; set; } = "{}";
    }

    /// <summary>Kết quả chấm điểm từ Gemini cho 1 application.</summary>
    public class AiScoringResultDto
    {
        public int Id { get; set; }
        public decimal AiScore { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}
