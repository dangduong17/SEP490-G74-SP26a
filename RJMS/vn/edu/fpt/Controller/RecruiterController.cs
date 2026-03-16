using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using System.Text.Json;

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
            {
                TempData["WarningToast"] = "Vui lòng đăng nhập bằng tài khoản Nhà tuyển dụng.";
                return RedirectToAction("Login", "Auth");
            }
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
        public async Task<IActionResult> JobPostingList(string? status, string? keyword, int page = 1)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userIdStr = Request.Cookies["UserId"];
            if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return NotFound("Không tìm thấy thông tin nhà tuyển dụng.");

            var query = _context.Jobs
                .Include(j => j.JobCategory)
                .Include(j => j.Location)
                .Where(j => j.RecruiterId == recruiter.Id);

            // Filtering
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword));
            }

            if (string.IsNullOrEmpty(status))
            {
                status = "Active"; // Default
            }

            if (status != "All")
            {
                query = query.Where(j => j.Status == status);
            }

            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new JobListItemDTO
                {
                    Id = j.Id,
                    Title = j.Title,
                    LocationName = j.Location != null ? j.Location.CityName : "Chưa rõ",
                    MinSalary = j.MinSalary,
                    MaxSalary = j.MaxSalary,
                    CreatedAt = j.CreatedAt,
                    ApplicationDeadline = j.ApplicationDeadline,
                    ExpiryDate = j.ExpiryDate,
                    Status = j.Status,
                    ApplicationCount = j.ApplicationCount ?? 0
                })
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.Keyword = keyword;
            ViewData["Title"] = "Quản lý tin tuyển dụng";

            return View(jobs);
        }

        // ── Create Job ─────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userIdStr = Request.Cookies["UserId"];
            int.TryParse(userIdStr, out int userId);

            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return Unauthorized();

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == recruiter.Id);
            if (job == null) return NotFound();

            // Soft delete: update status to Inactive
            job.Status = "Inactive";
            await _context.SaveChangesAsync();

            TempData["SuccessToast"] = "Đã tạm dừng tin tuyển dụng thành công.";
            return RedirectToAction(nameof(JobPostingList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateJob(int id)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userIdStr = Request.Cookies["UserId"];
            int.TryParse(userIdStr, out int userId);

            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return Unauthorized();

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == recruiter.Id);
            if (job == null) return NotFound();

            if (job.ExpiryDate.HasValue && job.ExpiryDate.Value < DateTime.Now)
            {
                TempData["ErrorToast"] = "Tin đã quá hạn, vui lòng chỉnh sửa ngày hết hạn trước khi hiển thị lại.";
                return RedirectToAction(nameof(JobPostingList));
            }

            job.Status = "Active";
            await _context.SaveChangesAsync();

            TempData["SuccessToast"] = "Đã hiển thị lại tin tuyển dụng thành công.";
            return RedirectToAction(nameof(JobPostingList));
        }

        // ── Edit Job ─────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userIdStr = Request.Cookies["UserId"];
            int.TryParse(userIdStr, out int userId);

            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return Unauthorized();

            var job = await _context.Jobs
                .Include(j => j.JobCategory)
                .Include(j => j.Location)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == recruiter.Id);

            if (job == null) return NotFound();

            var model = new JobSaveDTO
            {
                Id = job.Id,
                Title = job.Title,
                JobCategoryId = job.JobCategoryId ?? 0,
                JobType = job.JobType ?? "",
                NumberOfPositions = job.NumberOfPositions,
                MinSalary = job.MinSalary,
                MaxSalary = job.MaxSalary,
                Description = job.Description ?? "",
                Requirements = job.Requirements ?? "",
                Benefits = job.Benefits ?? "",
                ApplicationDeadline = job.ApplicationDeadline ?? DateTime.Now.AddDays(30),
                ExpiryDate = job.ExpiryDate ?? DateTime.Now.AddDays(30),
                ProvinceCode = job.Location?.ProvinceCode,
                ProvinceName = job.Location?.CityName,
                WardCode = job.Location?.WardCode,
                WardName = job.Location?.WardName,
                Address = job.Location?.Address,
                SelectedSkillIds = job.JobSkills.Select(js => js.SkillId).ToList()
            };

            // Handling category hierarchy
            if (job.JobCategory != null)
            {
                var cat = job.JobCategory;
                if (cat.Level == 3)
                {
                    model.JobCategoryLevel3Id = cat.Id;
                    model.JobCategoryLevel2Id = cat.ParentId;
                    var parent = await _context.JobCategories.FindAsync(cat.ParentId);
                    model.JobCategoryLevel1Id = parent?.ParentId;
                }
                else if (cat.Level == 2)
                {
                    model.JobCategoryLevel2Id = cat.Id;
                    model.JobCategoryLevel1Id = cat.ParentId;
                }
                else if (cat.Level == 1)
                {
                    model.JobCategoryLevel1Id = cat.Id;
                }
            }

            ViewBag.Categories1 = await _context.JobCategories.Where(c => c.Level == 1).OrderBy(c => c.Name).ToListAsync();
            ViewBag.Skills = await _context.Skills.OrderBy(s => s.Name).ToListAsync();
            ViewData["Title"] = "Chỉnh sửa tin tuyển dụng";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(JobSaveDTO model)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userIdStr = Request.Cookies["UserId"];
            int.TryParse(userIdStr, out int userId);

            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return Unauthorized();

            var job = await _context.Jobs
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.Id == model.Id && j.RecruiterId == recruiter.Id);

            if (job == null) return NotFound();

            bool isDraft = model.ActionType == "Draft";
            // Note: We don't necessarily decrement usage when editing, 
            // but if it was Draft and now is Submit, we should check/increment.
            if (job.Status == "Draft" && !isDraft)
            {
                // Similar subscription check logic as CreateJob...
                // (Simplified for now, assuming editing active job doesn't cost extra)
            }

            // Create or find Location
            var location = await _context.Locations.FirstOrDefaultAsync(l => 
                l.ProvinceCode == model.ProvinceCode && 
                l.WardCode == model.WardCode && 
                l.Address == model.Address);

            if (location == null)
            {
                location = new Location
                {
                    CityName = model.ProvinceName ?? "Chưa rõ",
                    ProvinceCode = model.ProvinceCode,
                    WardCode = model.WardCode,
                    WardName = model.WardName,
                    Address = model.Address
                };
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
            }

            if (model.ApplicationDeadline.Date > DateTime.Now.Date.AddDays(30))
            {
                ModelState.AddModelError("ApplicationDeadline", "Hạn nộp hồ sơ tối đa là 30 ngày kể từ thời điểm hiện tại.");
            }

            if (model.ExpiryDate.Date > DateTime.Now.Date.AddDays(30))
            {
                ModelState.AddModelError("ExpiryDate", "Ngày hết hạn tin tối đa là 30 ngày kể từ thời điểm hiện tại.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorToast"] = "Vui lòng kiểm tra lại thông tin!";
                ViewBag.Categories1 = await _context.JobCategories.Where(c => c.Level == 1).OrderBy(c => c.Name).ToListAsync();
                ViewBag.Skills = await _context.Skills.OrderBy(s => s.Name).ToListAsync();
                return View(model);
            }

            job.Title = model.Title;
            job.JobCategoryId = model.JobCategoryLevel3Id ?? model.JobCategoryLevel2Id ?? model.JobCategoryLevel1Id ?? model.JobCategoryId;
            job.JobType = model.JobType;
            job.NumberOfPositions = model.NumberOfPositions;
            job.MinSalary = model.MinSalary;
            job.MaxSalary = model.MaxSalary;
            job.Description = model.Description;
            job.Requirements = model.Requirements;
            job.Benefits = model.Benefits;
            job.ApplicationDeadline = model.ApplicationDeadline;
            job.ExpiryDate = model.ExpiryDate;
            job.LocationId = location.Id;
            job.Status = isDraft ? "Draft" : "Active";

            // Update Skills
            _context.JobSkills.RemoveRange(job.JobSkills);
            if (model.SelectedSkillIds != null)
            {
                foreach (var sId in model.SelectedSkillIds)
                {
                    _context.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = sId, IsRequired = true });
                }
            }

            if (!string.IsNullOrEmpty(model.NewSkills))
            {
                var skillNames = model.NewSkills.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Take(5);
                foreach (var name in skillNames)
                {
                    var existingSkill = await _context.Skills.FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
                    if (existingSkill == null)
                    {
                        existingSkill = new Skill { Name = name, Category = "Khác" };
                        _context.Skills.Add(existingSkill);
                        await _context.SaveChangesAsync();
                    }
                    _context.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = existingSkill.Id, IsRequired = true });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessToast"] = "Cập nhật tin tuyển dụng thành công!";
            return RedirectToAction(nameof(JobPostingList));
        }

        [HttpGet]
        public async Task<IActionResult> CreateJob()
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userIdStr = Request.Cookies["UserId"];
            int.TryParse(userIdStr, out int userId);

            // Fetch recruiter's company for default location
            var recruiter = await _context.Recruiters
                .Include(r => r.Company)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            var model = new JobSaveDTO
            {
                ApplicationDeadline = DateTime.Now.AddDays(30),
                ExpiryDate = DateTime.Now.AddDays(30)
            };

            if (recruiter?.Company != null)
            {
                model.ProvinceCode = recruiter.Company.ProvinceCode;
                model.ProvinceName = recruiter.Company.ProvinceName;
                model.WardCode = recruiter.Company.WardCode;
                model.WardName = recruiter.Company.WardName;
                model.Address = recruiter.Company.Address;
            }

            ViewBag.Categories1 = await _context.JobCategories.Where(c => c.Level == 1).OrderBy(c => c.Name).ToListAsync();
            ViewBag.Skills = await _context.Skills.OrderBy(s => s.Name).ToListAsync();
            ViewData["Title"] = "Đăng tin tuyển dụng";

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob(JobSaveDTO model)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userIdStr = Request.Cookies["UserId"];
            int.TryParse(userIdStr, out int userId);

            var recruiter = await _context.Recruiters
                .Include(r => r.Company)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter == null) return Unauthorized();

            bool isDraft = model.ActionType == "Draft";

            if (!isDraft)
            {
                // Check Subscription limit for JOB_POSTING
                var activeSubscription = await _context.Subscriptions
                    .Where(s => s.UserId == userId && s.Status == "Active")
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

                if (activeSubscription != null)
                {
                    var activePeriod = await _context.SubscriptionPeriods
                        .Include(p => p.SubscriptionUsages)
                        .Where(p => p.SubscriptionId == activeSubscription.Id)
                        .OrderByDescending(p => p.PeriodEnd)
                        .FirstOrDefaultAsync();

                    var planFeature = await _context.PlanFeatures
                        .FirstOrDefaultAsync(pf => pf.PlanId == activeSubscription.PlanId && pf.FeatureCode == "JOB_POSTING");

                    if (activePeriod != null && planFeature != null)
                    {
                        var usage = activePeriod.SubscriptionUsages.FirstOrDefault(u => u.FeatureCode == "JOB_POSTING");
                        int currentCount = usage?.UsedCount ?? 0;
                        int limit = planFeature.FeatureLimit ?? 0;

                        if (currentCount >= limit)
                        {
                            // Exceeded, forced to draft or return error
                            TempData["ErrorToast"] = "Bạn đã hết lượt đăng tin trong gói hiện tại. Tin đã được lưu ở dạng nháp.";
                            ModelState.AddModelError(string.Empty, "Bạn đã hết lượt đăng tin trong gói hiện tại. Tin đã được lưu ở dạng nháp.");
                            isDraft = true;
                        }
                        else
                        {
                            // Increment usage
                            if (usage == null)
                            {
                                usage = new SubscriptionUsage
                                {
                                    PeriodId = activePeriod.Id,
                                    FeatureCode = "JOB_POSTING",
                                    UsedCount = 1
                                };
                                _context.SubscriptionUsages.Add(usage);
                            }
                            else
                            {
                                usage.UsedCount += 1;
                            }
                        }
                    }
                    else
                    {
                        isDraft = true;
                        TempData["ErrorToast"] = "Gói đăng ký không hỗ trợ đăng tin. Tin đã lưu nháp.";
                        ModelState.AddModelError(string.Empty, "Gói đăng ký không hỗ trợ đăng tin. Tin đã lưu nháp.");
                    }
                }
                else
                {
                    isDraft = true;
                    TempData["ErrorToast"] = "Bạn chưa có gói đăng ký nào. Tin đã được lưu ở dạng nháp.";
                    ModelState.AddModelError(string.Empty, "Bạn chưa có gói đăng ký nào. Tin đã được lưu ở dạng nháp.");
                }
            }

            // Create or find Location
            var location = await _context.Locations.FirstOrDefaultAsync(l => 
                l.ProvinceCode == model.ProvinceCode && 
                l.WardCode == model.WardCode && 
                l.Address == model.Address);

            if (location == null)
            {
                location = new Location
                {
                    CityName = model.ProvinceName ?? "Chưa rõ",
                    ProvinceCode = model.ProvinceCode,
                    WardCode = model.WardCode,
                    WardName = model.WardName,
                    Address = model.Address
                };
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
            }

            // Application Deadline Validation
            if (model.ApplicationDeadline.Date > DateTime.Now.Date.AddDays(30))
            {
                ModelState.AddModelError("ApplicationDeadline", "Hạn nộp hồ sơ tối đa là 30 ngày kể từ thời điểm hiện tại.");
            }

            // Determine specific CategoryId
            int finalCategoryId = model.JobCategoryLevel3Id ?? model.JobCategoryLevel2Id ?? model.JobCategoryLevel1Id ?? model.JobCategoryId;
            if (finalCategoryId == 0)
            {
                ModelState.AddModelError("JobCategoryId", "Vui lòng chọn hoặc thêm nhóm nghề.");
            }

            // Application Deadline & Expiry Validation
            if (model.ApplicationDeadline.Date > DateTime.Now.Date.AddDays(30))
            {
                ModelState.AddModelError("ApplicationDeadline", "Hạn nộp hồ sơ tối đa là 30 ngày kể từ thời điểm hiện tại.");
            }
            if (model.ExpiryDate.Date > DateTime.Now.Date.AddDays(30))
            {
                ModelState.AddModelError("ExpiryDate", "Ngày hết hạn tin tối đa là 30 ngày kể từ thời điểm hiện tại.");
            }

            if (!ModelState.IsValid) 
            {
                TempData["ErrorToast"] = "Vui lòng kiểm tra lại thông tin đăng tin!";
                ViewBag.Categories1 = await _context.JobCategories.Where(c => c.Level == 1).OrderBy(c => c.Name).ToListAsync();
                ViewBag.Skills = await _context.Skills.OrderBy(s => s.Name).ToListAsync();
                return View(model);
            }

            var job = new Job
            {
                Title = model.Title,
                CompanyId = recruiter.CompanyId ?? 0,
                RecruiterId = recruiter.Id,
                JobCategoryId = finalCategoryId,
                LocationId = location.Id,
                Description = model.Description,
                Requirements = model.Requirements,
                Benefits = model.Benefits,
                JobType = model.JobType,
                MinSalary = model.MinSalary,
                MaxSalary = model.MaxSalary,
                NumberOfPositions = model.NumberOfPositions,
                ApplicationDeadline = model.ApplicationDeadline == default ? DateTime.Now.AddDays(30) : model.ApplicationDeadline,
                ExpiryDate = model.ExpiryDate == default ? DateTime.Now.AddDays(30) : model.ExpiryDate,
                Status = isDraft ? "Draft" : "Active",
                CreatedAt = DateTime.Now
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            // Process Skills
            if (model.SelectedSkillIds != null)
            {
                foreach (var sId in model.SelectedSkillIds.Take(5)) // max 5
                {
                    _context.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = sId, IsRequired = true });
                }
            }

            if (!string.IsNullOrWhiteSpace(model.NewSkills))
            {
                var newSkList = model.NewSkills.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct().Take(5).ToList();
                foreach (var ns in newSkList)
                {
                    var existingSkill = await _context.Skills.FirstOrDefaultAsync(s => s.Name.ToLower() == ns.ToLower());
                    if (existingSkill == null)
                    {
                        existingSkill = new Skill { Name = ns, Category = "Khác" };
                        _context.Skills.Add(existingSkill);
                        await _context.SaveChangesAsync();
                    }
                    if (!model.SelectedSkillIds?.Contains(existingSkill.Id) ?? true)
                    {
                        _context.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = existingSkill.Id, IsRequired = true });
                    }
                }
            }
            await _context.SaveChangesAsync();

            TempData["SuccessToast"] = isDraft ? "Lưu nháp thành công!" : "Đăng tin thành công!";
            return RedirectToAction("JobPostingList");
        }

        [HttpGet]
        public async Task<IActionResult> GetJobCategories(int? parentId, int level = 1)
        {
            var query = _context.JobCategories.Where(c => c.Level == level);
            if (parentId.HasValue) query = query.Where(c => c.ParentId == parentId.Value);
            else if (level > 1) query = query.Where(c => false);

            var categories = await query.Select(c => new { c.Id, c.Name }).OrderBy(c => c.Name).ToListAsync();
            return Json(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobCategoryApi([FromBody] JsonElement payload)
        {
            if (RequireRecruiter() != null) return Unauthorized();

            string name = payload.GetProperty("name").GetString() ?? "";
            int level = payload.TryGetProperty("level", out var lProp) ? lProp.GetInt32() : 1;
            int? parentId = payload.TryGetProperty("parentId", out var pProp) && pProp.ValueKind != JsonValueKind.Null ? pProp.GetInt32() : null;
            
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Tên danh mục không hợp lệ");

            var query = _context.JobCategories.Where(c => c.Name.ToLower() == name.ToLower() && c.Level == level);
            if (parentId.HasValue) query = query.Where(c => c.ParentId == parentId);

            var exists = await query.AnyAsync();
            if (exists)
            {
                var existing = await query.FirstAsync();
                return Json(new { success = true, id = existing.Id, name = existing.Name });
            }

            var category = new JobCategory
            {
                Name = name,
                Level = level,
                ParentId = parentId,
                CreatedAt = DateTime.Now
            };

            _context.JobCategories.Add(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = category.Id, name = category.Name });
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
