using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class WebSliderService : IWebSliderService
    {
        private readonly FindingJobsDbContext _db;
        private readonly ICloudinaryService _cloudinary;

        public WebSliderService(FindingJobsDbContext db, ICloudinaryService cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        // ── List (Manager) ───────────────────────────────────────────────────────
        public async Task<WebSliderListViewModel> GetListAsync(string? keyword, string? statusFilter, int page, int pageSize)
        {
            var query = _db.WebSliders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(s => s.Title.Contains(keyword) || (s.Subtitle != null && s.Subtitle.Contains(keyword)));

            if (statusFilter == "active")
                query = query.Where(s => s.IsActive);
            else if (statusFilter == "inactive")
                query = query.Where(s => !s.IsActive);

            var total = await query.CountAsync();

            var sliders = await query
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new WebSliderDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Subtitle = s.Subtitle,
                    ImageUrl = s.ImageUrl,
                    LinkUrl = s.LinkUrl,
                    ButtonText = s.ButtonText,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return new WebSliderListViewModel
            {
                Keyword = keyword,
                StatusFilter = statusFilter,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Sliders = sliders
            };
        }

        // ── Active sliders for Home page ─────────────────────────────────────────
        public async Task<List<WebSliderDto>> GetActiveForDisplayAsync()
        {
            var now = DateTimeHelper.NowVietnam;
            return await _db.WebSliders
                .Where(s => s.IsActive
                    && (s.StartDate == null || s.StartDate <= now)
                    && (s.EndDate == null || s.EndDate >= now))
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new WebSliderDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Subtitle = s.Subtitle,
                    ImageUrl = s.ImageUrl,
                    LinkUrl = s.LinkUrl,
                    ButtonText = s.ButtonText,
                    DisplayOrder = s.DisplayOrder,
                    IsActive = s.IsActive,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();
        }

        // ── Get for edit ─────────────────────────────────────────────────────────
        public async Task<WebSliderFormViewModel?> GetForEditAsync(int id)
        {
            var s = await _db.WebSliders.FindAsync(id);
            if (s == null) return null;

            return new WebSliderFormViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Subtitle = s.Subtitle,
                ImageUrl = s.ImageUrl,
                ExistingImageUrl = s.ImageUrl,
                LinkUrl = s.LinkUrl,
                ButtonText = s.ButtonText,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            };
        }

        // ── Create ───────────────────────────────────────────────────────────────
        public async Task<(bool success, string message)> CreateAsync(WebSliderFormViewModel form)
        {
            if (form.ImageFile == null)
                return (false, "Vui lòng chọn ảnh cho slider.");

            var imageUrl = await _cloudinary.UploadImageAsync(form.ImageFile, "sliders");
            if (string.IsNullOrEmpty(imageUrl))
                return (false, "Upload ảnh thất bại. Vui lòng thử lại.");

            var slider = new WebSlider
            {
                Title = form.Title,
                Subtitle = form.Subtitle,
                ImageUrl = imageUrl,
                LinkUrl = form.LinkUrl,
                ButtonText = form.ButtonText,
                DisplayOrder = form.DisplayOrder,
                IsActive = form.IsActive,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                CreatedAt = DateTimeHelper.NowVietnam
            };

            _db.WebSliders.Add(slider);
            await _db.SaveChangesAsync();
            return (true, "Tạo slider thành công.");
        }

        // ── Update ───────────────────────────────────────────────────────────────
        public async Task<(bool success, string message)> UpdateAsync(WebSliderFormViewModel form)
        {
            var slider = await _db.WebSliders.FindAsync(form.Id);
            if (slider == null) return (false, "Không tìm thấy slider.");

            // Upload new image if provided
            if (form.ImageFile != null)
            {
                var imageUrl = await _cloudinary.UploadImageAsync(form.ImageFile, "sliders");
                if (string.IsNullOrEmpty(imageUrl))
                    return (false, "Upload ảnh thất bại. Vui lòng thử lại.");
                slider.ImageUrl = imageUrl;
            }

            slider.Title = form.Title;
            slider.Subtitle = form.Subtitle;
            slider.LinkUrl = form.LinkUrl;
            slider.ButtonText = form.ButtonText;
            slider.DisplayOrder = form.DisplayOrder;
            slider.IsActive = form.IsActive;
            slider.StartDate = form.StartDate;
            slider.EndDate = form.EndDate;
            slider.UpdatedAt = DateTimeHelper.NowVietnam;

            await _db.SaveChangesAsync();
            return (true, "Cập nhật slider thành công.");
        }

        // ── Delete ───────────────────────────────────────────────────────────────
        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            var slider = await _db.WebSliders.FindAsync(id);
            if (slider == null) return (false, "Không tìm thấy slider.");

            _db.WebSliders.Remove(slider);
            await _db.SaveChangesAsync();
            return (true, "Đã xóa slider.");
        }

        // ── Hangfire: expire outdated sliders ────────────────────────────────────
        public async Task<int> ExpireOutdatedAsync()
        {
            var now = DateTimeHelper.NowVietnam;
            var outdated = await _db.WebSliders
                .Where(s => s.IsActive && s.EndDate.HasValue && s.EndDate < now)
                .ToListAsync();

            foreach (var s in outdated)
            {
                s.IsActive = false;
                s.UpdatedAt = now;
            }

            if (outdated.Any())
                await _db.SaveChangesAsync();

            return outdated.Count;
        }
    }
}
