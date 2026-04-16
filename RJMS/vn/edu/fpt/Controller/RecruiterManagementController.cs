using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class RecruiterManagementController : Controller
    {
        private readonly IRecruiterManagementService _svc;
        private readonly FindingJobsDbContext _db;

        public RecruiterManagementController(IRecruiterManagementService svc, FindingJobsDbContext db)
        {
            _svc = svc;
            _db = db;
        }

        private IActionResult? RequireRecruiter(out int userId)
        {
            userId = 0;
            var role = Request.Cookies["UserRole"];
            if (role != "Recruiter" && role != "Employee")
                return RedirectToAction("Login", "Auth");
            var idStr = Request.Cookies["UserId"];
            if (!int.TryParse(idStr, out userId))
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // ── COMPANY LOCATIONS ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CompanyLocations()
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var model = await _svc.GetCompanyLocationsAsync(userId);
            if (model == null)
            {
                TempData["ErrorToast"] = "Bạn chưa liên kết với công ty nào.";
                return RedirectToAction("Index", "Recruiter");
            }
            ViewData["Title"] = "Quản lý địa chỉ công ty";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLocation(RecruiterAddLocationViewModel model)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var (ok, err) = await _svc.AddCompanyLocationAsync(userId, model);
            TempData[ok ? "SuccessToast" : "ErrorToast"] = ok ? "Đã thêm địa chỉ thành công." : err;
            return RedirectToAction(nameof(CompanyLocations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            // Check ownership + employee count before deleting
            var recruiter = await _db.Recruiters.AsNoTracking().FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter?.CompanyId == null) return Forbid();

            var cl = await _db.CompanyLocations
                .Include(c => c.RecruiterLocations)
                .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == recruiter.CompanyId);

            if (cl == null) return NotFound();
            if (cl.RecruiterLocations.Any())
            {
                TempData["ErrorToast"] = "Không thể xóa địa chỉ đang có nhân viên được gán.";
                return RedirectToAction(nameof(CompanyLocations));
            }

            _db.CompanyLocations.Remove(cl);
            await _db.SaveChangesAsync();
            TempData["SuccessToast"] = "Đã xóa địa chỉ thành công.";
            return RedirectToAction(nameof(CompanyLocations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryLocation(int id)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var (ok, err) = await _svc.SetPrimaryLocationAsync(userId, id);
            TempData[ok ? "SuccessToast" : "ErrorToast"] = ok ? "Đã đặt địa chỉ chính thành công." : err;
            return RedirectToAction(nameof(CompanyLocations));
        }

        // ── EMPLOYEES ──────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Employees(string? keyword, int page = 1)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var model = await _svc.GetEmployeeListAsync(userId, keyword, page, 10);
            if (model == null)
            {
                TempData["ErrorToast"] = "Bạn chưa liên kết với công ty nào.";
                return RedirectToAction("Index", "Recruiter");
            }
            ViewData["Title"] = "Quản lý nhân viên";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(RecruiterCreateEmployeeViewModel model)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var (ok, err) = await _svc.CreateEmployeeAsync(userId, model);
            TempData[ok ? "SuccessToast" : "ErrorToast"] = ok ? $"Đã tạo tài khoản cho {model.FirstName} {model.LastName} thành công." : err;
            return RedirectToAction(nameof(Employees));
        }

        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var model = await _svc.GetEmployeeForEditAsync(userId, id);
            if (model == null) return NotFound();
            return Json(model); // AJAX-driven modal
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(RecruiterEditEmployeeViewModel model)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var (ok, err) = await _svc.UpdateEmployeeAsync(userId, model);
            TempData[ok ? "SuccessToast" : "ErrorToast"] = ok ? "Cập nhật nhân viên thành công." : err;
            return RedirectToAction(nameof(Employees));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanEmployee(int id)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var (ok, err) = await _svc.BanEmployeeAsync(userId, id);
            TempData[ok ? "SuccessToast" : "ErrorToast"] = ok ? "Đã khóa tài khoản nhân viên." : err;
            return RedirectToAction(nameof(Employees));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanEmployee(int id)
        {
            var guard = RequireRecruiter(out int userId);
            if (guard != null) return guard;

            var (ok, err) = await _svc.UnbanEmployeeAsync(userId, id);
            TempData[ok ? "SuccessToast" : "ErrorToast"] = ok ? "Đã mở khóa tài khoản nhân viên." : err;
            return RedirectToAction(nameof(Employees));
        }
    }
}
