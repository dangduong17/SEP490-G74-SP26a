using System;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class CandidateDashboardDTO
    {
        public string UserId { get; set; } = string.Empty;
        public int TotalApplications { get; set; }
        public int InterviewsScheduled { get; set; }
        public int OffersReceived { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
