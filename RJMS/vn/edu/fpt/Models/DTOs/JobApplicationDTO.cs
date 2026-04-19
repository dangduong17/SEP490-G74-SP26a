using System;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class JobApplicationDTO
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int CvId { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public string CustomTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogo { get; set; }
        public string? CvTitle { get; set; }
        public string? CvType { get; set; }
        public string? CvFileUrl { get; set; }
        public string? CvFileName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }
}
