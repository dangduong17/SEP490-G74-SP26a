namespace RJMS.vn.edu.fpt.Models.DTOs
{
    /// <summary>
    /// ViewModel cho trang Recruiter Dashboard.
    /// Hiện tại dùng dữ liệu mẫu (template). Tích hợp service thực sau khi có DB queries.
    /// </summary>
    public class RecruiterDashboardViewModel
    {
        // ---- Stat cards ----
        public int ActiveJobPosts { get; set; } = 12;
        public int TotalApplications { get; set; } = 848;
        public int InterviewsScheduled { get; set; } = 24;
        public int ProfileViews { get; set; } = 2400;

        // ---- Recruiter info (from cookie / session) ----
        public string RecruiterName { get; set; } = "HR Manager";
        public string CompanyName { get; set; } = "Finding Jobs";

        // ---- Subscription ----
        public string SubscriptionPlan { get; set; } = "Premium Plan";
        public string SubscriptionValidTo { get; set; } = "31 Dec 2025";
        public decimal PlanPrice { get; set; } = 0;
        public int JobPostsUsed { get; set; } = 8;
        public int JobPostsTotal { get; set; } = 20;
        public int CvSearchesUsed { get; set; } = 156;
        public int CvSearchesTotal { get; set; } = 500;

        // ---- Recent Applications ----
        public List<RecentApplicationItem> RecentApplications { get; set; } = new()
        {
            new() { CandidateName = "Nguyễn Minh Anh",  JobTitle = "Senior Frontend Developer",  AppliedDate = "2023-10-24", Status = "New",       StatusClass = "status-new" },
            new() { CandidateName = "Lê Hoàng Long",    JobTitle = "Product Designer (UI/UX)",    AppliedDate = "2023-10-23", Status = "Reviewing",  StatusClass = "status-reviewing" },
            new() { CandidateName = "Phạm Thùy Linh",  JobTitle = "Marketing Manager",           AppliedDate = "2023-10-22", Status = "Interview",  StatusClass = "status-interview" },
            new() { CandidateName = "Trần Văn Đạt",    JobTitle = "Backend Developer (NodeJS)",  AppliedDate = "2023-10-21", Status = "Hired",      StatusClass = "status-hired" },
            new() { CandidateName = "Vũ Thị Hoa",      JobTitle = "HR Specialist",               AppliedDate = "2023-10-20", Status = "Rejected",   StatusClass = "status-rejected" },
        };

        // ---- Recent Job Posts ----
        public List<RecentJobPostItem> RecentJobPosts { get; set; } = new()
        {
            new() { JobTitle = "Senior Frontend Developer",   PostedDate = "2023-10-15", ExpiryDate = "2023-11-15", Status = "Active",  StatusClass = "status-active-post" },
            new() { JobTitle = "Product Designer (UI/UX)",   PostedDate = "2023-10-12", ExpiryDate = "2023-11-12", Status = "Active",  StatusClass = "status-active-post" },
            new() { JobTitle = "Backend Developer (NodeJS)", PostedDate = "2023-10-05", ExpiryDate = "2023-11-05", Status = "Active",  StatusClass = "status-active-post" },
            new() { JobTitle = "Sales Executive",            PostedDate = "2023-09-28", ExpiryDate = "2023-10-28", Status = "Expired", StatusClass = "status-expired-post" },
            new() { JobTitle = "Graphic Designer",           PostedDate = "2023-09-20", ExpiryDate = "2023-10-20", Status = "Active",  StatusClass = "status-active-post" },
        };
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
