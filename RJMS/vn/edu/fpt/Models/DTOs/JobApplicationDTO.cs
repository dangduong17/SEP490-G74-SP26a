using System;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class JobApplicationDTO
    {
        public int Id { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }
}
