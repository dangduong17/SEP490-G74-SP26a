namespace RJMS.vn.edu.fpt.Models.DTOs
{
    /// <summary>
    /// ViewModel cho trang Recruiter Dashboard.
    /// Dữ liệu được đổ trực tiếp từ database theo role và công ty.
    /// </summary>
    public class RecruiterDashboardViewModel
    {
        // ---- Stat cards ----
        public int ActiveJobPosts { get; set; }
        public int TotalApplications { get; set; }
        public int InterviewsScheduled { get; set; }
        public int FollowerCount { get; set; } = 0;

        // ---- Recruiter info (from cookie / session) ----
        public string RecruiterName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public List<RecruiterCompanyLocationViewModel> CompanyLocations { get; set; } = new();

        // ---- Subscription ----
        public string SubscriptionPlan { get; set; } = string.Empty;
        public string SubscriptionValidTo { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public int JobPostsUsed { get; set; }
        public int JobPostsTotal { get; set; }
        public int CvSearchesUsed { get; set; }
        public int CvSearchesTotal { get; set; }

        // ---- Recent Applications ----
        public List<RecentApplicationItem> RecentApplications { get; set; } = new();

        // ---- Recent Job Posts ----
        public List<RecentJobPostItem> RecentJobPosts { get; set; } = new();
    }

    public class RecentApplicationItem
    {
        public int Id { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string AppliedDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public int CvId { get; set; }
        public string Initials => CandidateName.Length > 0 ? CandidateName[0].ToString().ToUpper() : "?";
    }

    public class RecentJobPostItem
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string PostedDate { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
    }
}
