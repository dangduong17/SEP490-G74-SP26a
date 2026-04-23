using System;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    public class ApplicationListItemDTO
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string? CandidateEmail { get; set; }
        public string? CandidatePhone { get; set; }
        public string? CandidateAvatar { get; set; }
        public DateTime? AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CoverLetter { get; set; }
        public int CvId { get; set; }
        public decimal? AiScore { get; set; }
        public string? AiProcessStatus { get; set; }
        public string? MatchedSkills { get; set; }
        public string? MissingSkills { get; set; }
        public string? Summary { get; set; }
    }
}
