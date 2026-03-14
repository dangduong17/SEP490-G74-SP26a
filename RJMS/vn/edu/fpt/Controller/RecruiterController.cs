using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class RecruiterController : Controller
    {
        private readonly FindingJobsDbContext _context;

        public RecruiterController(FindingJobsDbContext context)
        {
            _context = context;
        }

        // ── Auth guard ────────────────────────────────────────────────────────────
        private IActionResult? RequireRecruiter()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Recruiter")
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // ── Dashboard (GET /Recruiter) ─────────────────────────────────────────
        public async Task<IActionResult> RecruiterDashboard()
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            // Lấy thông tin recruiter từ cookie (hoặc tích hợp service sau)
            var userName = Request.Cookies["UserName"] ?? "Recruiter";
            var userIdStr = Request.Cookies["UserId"];
            int userId = 0;
            int.TryParse(userIdStr, out userId);

            var model = new RecruiterDashboardViewModel
            {
                RecruiterName = userName,
            };

            if (userId > 0)
            {
                var activeSubscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.UserId == userId && s.Status == "Active")
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

                if (activeSubscription != null)
                {
                    model.SubscriptionPlan = activeSubscription.Plan.Name;
                    model.SubscriptionValidTo = activeSubscription.EndDate.HasValue ? activeSubscription.EndDate.Value.ToString("dd/MM/yyyy") : "Không thời hạn";

                    model.PlanPrice = activeSubscription.Plan.Price ?? 0;

                    // Fetch active Period
                    var activePeriod = await _context.SubscriptionPeriods
                        .Include(p => p.SubscriptionUsages)
                        .Where(p => p.SubscriptionId == activeSubscription.Id)
                        .OrderByDescending(p => p.PeriodEnd)
                        .FirstOrDefaultAsync();
                        
                    var planFeatures = await _context.PlanFeatures.Where(pf => pf.PlanId == activeSubscription.PlanId).ToListAsync();

                    if (activePeriod != null && activePeriod.SubscriptionUsages != null)
                    {
                        var usages = activePeriod.SubscriptionUsages;

                        var jobFeature = planFeatures.FirstOrDefault(f => f.FeatureCode == "JOB_POSTING");
                        var jobUsage = usages.FirstOrDefault(u => u.FeatureCode == "JOB_POSTING");
                        model.JobPostsTotal = jobFeature?.FeatureLimit ?? 0;
                        model.JobPostsUsed = model.JobPostsTotal - (jobUsage?.UsedCount ?? 0); // "Lượt đăng tin còn lại"
                        if(model.JobPostsUsed < 0) model.JobPostsUsed = 0;

                        var detailFeature = planFeatures.FirstOrDefault(f => f.FeatureCode == "CV_AI_FILTER");
                        var detailUsage = usages.FirstOrDefault(u => u.FeatureCode == "CV_AI_FILTER");
                        model.CvSearchesTotal = detailFeature?.FeatureLimit ?? 0;
                        model.CvSearchesUsed = model.CvSearchesTotal - (detailUsage?.UsedCount ?? 0); // "Lượt tìm CV còn lại"
                        if(model.CvSearchesUsed < 0) model.CvSearchesUsed = 0;
                    }
                }
                else
                {
                    model.SubscriptionPlan = "Gói Miễn Phí";
                    model.SubscriptionValidTo = "Không có thời hạn";
                    model.PlanPrice = 0;
                }
            }

            ViewData["Title"] = "Recruiter Dashboard";
            return View(model);
        }

        // ── Job Posts ─────────────────────────────────────────────────────────
        public IActionResult JobPostingList()
        {
            if (RequireRecruiter() is { } redirect) return redirect;
            ViewData["Title"] = "Quản lý tin tuyển dụng";
            return View();
        }

        // ── Applications ──────────────────────────────────────────────────────
        public IActionResult Applications()
        {
            if (RequireRecruiter() is { } redirect) return redirect;
            ViewData["Title"] = "Danh sách ứng tuyển";
            return View();
        }

        // ── Candidates ────────────────────────────────────────────────────────
        public IActionResult Candidates()
        {
            if (RequireRecruiter() is { } redirect) return redirect;
            ViewData["Title"] = "Tìm ứng viên";
            return View();
        }

        // ── Messages ──────────────────────────────────────────────────────────
        public IActionResult Messages()
        {
            if (RequireRecruiter() is { } redirect) return redirect;
            ViewData["Title"] = "Tin nhắn";
            return View();
        }
    }
}
