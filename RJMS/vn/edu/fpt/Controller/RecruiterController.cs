using Microsoft.AspNetCore.Mvc;
using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class RecruiterController : Controller
    {
        // ── Auth guard ────────────────────────────────────────────────────────────
        private IActionResult? RequireRecruiter()
        {
            var role = Request.Cookies["UserRole"];
            if (role != "Recruiter")
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // ── Dashboard (GET /Recruiter) ─────────────────────────────────────────
        public IActionResult RecruiterDashboard()
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            // Lấy thông tin recruiter từ cookie (hoặc tích hợp service sau)
            var userName = Request.Cookies["UserName"] ?? "Recruiter";

            var model = new RecruiterDashboardViewModel
            {
                RecruiterName = userName,
            };

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
