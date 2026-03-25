using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using System.Text.Json;
using RJMS.Vn.Edu.Fpt.Service;

namespace RJMS.Vn.Edu.Fpt.Controllers
{
    public class RecruiterController : Controller
    {
        private readonly FindingJobsDbContext _context;
        private readonly ISubscriptionService _subscriptionService;

        public RecruiterController(FindingJobsDbContext context, ISubscriptionService subscriptionService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
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
                var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
                if (recruiter != null)
                {
                    model.ActiveJobPosts = await _context.Jobs.CountAsync(j => j.RecruiterId == recruiter.Id && j.Status == "Active");
                    model.TotalApplications = await _context.Jobs
                        .Where(j => j.RecruiterId == recruiter.Id)
                        .SumAsync(j => j.ApplicationCount ?? 0);
                    model.InterviewsScheduled = 0; // Future feature
                    model.ProfileViews = 0; // Future feature
                    
                    var recentJobs = await _context.Jobs
                        .Where(j => j.RecruiterId == recruiter.Id)
                        .OrderByDescending(j => j.CreatedAt)
                        .Take(5)
                        .Select(j => new RecentJobPostItem
                        {
                            Id = j.Id,
                            JobTitle = j.Title,
                            PostedDate = j.CreatedAt.HasValue ? j.CreatedAt.Value.ToString("dd/MM/yyyy") : "",
                            ExpiryDate = j.ExpiryDate.HasValue ? j.ExpiryDate.Value.ToString("dd/MM/yyyy") : "",
                            Status = j.Status == "Active" ? "Đang hiển thị" : (j.Status == "Scheduled" ? "Đã lên lịch" : (j.Status == "Draft" ? "Tin nháp" : "Tạm dừng")),
                            StatusClass = j.Status == "Active" ? "status-active-post" : (j.Status == "Draft" || j.Status == "Scheduled" ? "status-draft-post" : "status-expired-post")
                        })
                        .ToListAsync();
                    
                    model.RecentJobPosts = recentJobs;
                    
                    // Fetch recent applications - simplified for now
                    model.RecentApplications = await _context.Applications
                        .Include(a => a.Job)
                        .Include(a => a.Candidate)
                        .Where(a => a.Job.RecruiterId == recruiter.Id)
                        .OrderByDescending(a => a.CreatedAt)
                        .Take(5)
                        .Select(a => new RecentApplicationItem
                        {
                            Id = a.Id,
                            CandidateName = a.Candidate.FullName ?? "Ứng viên",
                            JobTitle = a.Job.Title,
                            AppliedDate = a.CreatedAt.HasValue ? a.CreatedAt.Value.ToString("dd/MM/yyyy") : "",
                            Status = a.Status ?? "Mới",
                            StatusClass = (a.Status == "Đang xem xét" || a.Status == "Reviewing") ? "status-reviewing" : 
                                          (a.Status == "Đặt lịch hẹn" || a.Status == "Interview") ? "status-interview" : 
                                          (a.Status == "Đã tuyển" || a.Status == "Hired") ? "status-hired" : 
                                          (a.Status == "Từ chối" || a.Status == "Rejected") ? "status-rejected" : "status-new",
                            CvId = a.Cvid
                        })
                        .ToListAsync();
                }

                var activeSubscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.UserId == userId && s.Status == "Active")
                    .OrderByDescending(s => s.EndDate)
                    .FirstOrDefaultAsync();

                if (activeSubscription != null && activeSubscription.Plan != null)
                {
                    model.SubscriptionPlan = activeSubscription.Plan.Name ?? "Chưa xác định";
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

            // Auto-update Scheduled to Active if time reached, and Active to Expired if expired
            var jobsToUpdate = await query.Where(j => 
                (j.Status == "Scheduled" && j.CreatedAt <= DateTime.Now) ||
                (j.Status == "Active" && j.ExpiryDate < DateTime.Now)).ToListAsync();

            if (jobsToUpdate.Any())
            {
                foreach(var jobUpdate in jobsToUpdate)
                {
                    if (jobUpdate.Status == "Scheduled" && jobUpdate.CreatedAt <= DateTime.Now)
                    {
                        jobUpdate.Status = "Active";
                    }
                    if (jobUpdate.Status == "Active" && jobUpdate.ExpiryDate < DateTime.Now)
                    {
                        jobUpdate.Status = "Expired";
                    }
                }
                await _context.SaveChangesAsync();
            }

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

            job.Status = (job.CreatedAt.HasValue && job.CreatedAt.Value > DateTime.Now) ? "Scheduled" : "Active";
            await _context.SaveChangesAsync();

            TempData["SuccessToast"] = job.Status == "Scheduled" ? "Đã lên lịch hiển thị tin tuyển dụng thành công." : "Đã hiển thị lại tin tuyển dụng thành công.";
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
                PublishDate = job.CreatedAt ?? DateTime.Now,
                Status = job.Status,
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

            if (model.PublishDate.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("PublishDate", "Ngày hiển thị không được trong quá khứ.");
            }

            if (model.PublishDate.Date > DateTime.Now.Date.AddDays(15))
            {
                ModelState.AddModelError("PublishDate", "Ngày hiển thị chỉ được thiết lập tối đa 15 ngày kể từ thời điểm hiện tại.");
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
            job.CreatedAt = model.PublishDate;
            job.LocationId = location.Id;

            // Preserve chosen Status if editing manually instead of submitting draft/saving
            if (isDraft)
            {
                job.Status = "Draft";
            }
            else
            {
                if (!string.IsNullOrEmpty(model.Status))
                {
                    job.Status = model.Status; // user manually picked status
                    // Force Scheduled if they selected Active but publish date is future
                    if ((job.Status == "Active" || job.Status == "Đang hiển thị") && model.PublishDate > DateTime.Now)
                    {
                        job.Status = "Scheduled";
                    }
                }
                else
                {
                    job.Status = model.PublishDate > DateTime.Now ? "Scheduled" : "Active";
                }
            }

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
                ExpiryDate = DateTime.Now.AddDays(30),
                PublishDate = DateTime.Now
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
                var quota = await _subscriptionService.CheckQuotaAsync(userId, "JOB_POSTING");
                if (!quota.Allowed)
                {
                    isDraft = true;
                    TempData["ErrorToast"] = quota.Message + " Tin đã được lưu ở dạng nháp.";
                    ModelState.AddModelError(string.Empty, quota.Message);
                }
                else
                {
                    // Everything OK, we will consume after save
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

            // Application Deadline & Expiry & Publish Date Validation
            if (model.ApplicationDeadline.Date > DateTime.Now.Date.AddDays(30))
            {
                ModelState.AddModelError("ApplicationDeadline", "Hạn nộp hồ sơ tối đa là 30 ngày kể từ thời điểm hiện tại.");
            }
            if (model.ExpiryDate.Date > DateTime.Now.Date.AddDays(30))
            {
                ModelState.AddModelError("ExpiryDate", "Ngày hết hạn tin tối đa là 30 ngày kể từ thời điểm hiện tại.");
            }
            if (model.PublishDate.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("PublishDate", "Ngày hiển thị không được trong quá khứ.");
            }
            if (model.PublishDate.Date > DateTime.Now.Date.AddDays(15))
            {
                ModelState.AddModelError("PublishDate", "Ngày hiển thị chỉ được thiết lập tối đa 15 ngày kể từ thời điểm hiện tại.");
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
                Status = isDraft ? "Draft" : (model.PublishDate > DateTime.Now ? "Scheduled" : "Active"),
                CreatedAt = model.PublishDate == default ? DateTime.Now : model.PublishDate
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

            // Spend quota after save
            if (!isDraft)
            {
                await _subscriptionService.ConsumeQuotaAsync(userId, "JOB_POSTING");
            }

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
        public async Task<IActionResult> Applications(int? jobId, string? status, string? keyword)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var userIdStr = Request.Cookies["UserId"];
            if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Auth");

            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return RedirectToAction("Login", "Auth");

            var query = _context.Applications
                .Include(a => a.Job)
                .Include(a => a.Candidate)
                .ThenInclude(c => c.User)
                .Where(a => a.Job.RecruiterId == recruiter.Id);

            // Filter
            if (jobId.HasValue) query = query.Where(a => a.JobId == jobId.Value);
            if (!string.IsNullOrEmpty(status) && status != "All") query = query.Where(a => a.Status == status);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => (a.Candidate.FullName ?? "").Contains(keyword) || 
                                       (a.Candidate.User.Email ?? "").Contains(keyword) ||
                                       (a.Job.Title ?? "").Contains(keyword));
            }

            var applications = await query
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new ApplicationListItemDTO
                {
                    Id = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    CandidateId = a.CandidateId,
                    CandidateName = a.Candidate.FullName ?? "Ứng viên",
                    CandidateEmail = a.Candidate.User.Email,
                    CandidatePhone = a.Candidate.Phone,
                    CandidateAvatar = a.Candidate.Avatar,
                    AppliedDate = a.CreatedAt,
                    Status = a.Status ?? "Mới",
                    CoverLetter = a.CoverLetter,
                    CvId = a.Cvid
                })
                .ToListAsync();

            ViewBag.Jobs = await _context.Jobs.Where(j => j.RecruiterId == recruiter.Id).Select(j => new { j.Id, j.Title }).ToListAsync();
            ViewBag.CurrentJobId = jobId;
            ViewBag.CurrentStatus = status;
            ViewBag.Keyword = keyword;

            ViewData["Title"] = "Danh sách ứng tuyển";
            return View(applications);
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
        [HttpPost]
        public async Task<IActionResult> UpdateApplicationStatus(int id, string status)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var application = await _context.Applications.FindAsync(id);
            if (application != null)
            {
                application.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessToast"] = $"Đã cập nhật trạng thái ứng viên thành '{status}'";
            }

            return RedirectToAction("Applications");
        }

        public async Task<IActionResult> ViewCV(int id)
        {
            if (RequireRecruiter() is { } redirect) return redirect;

            var cv = await _context.Cvs.FindAsync(id);
            var fileUrl = cv?.FileUrl ?? cv?.LegacyFilePath;
            if (cv == null || string.IsNullOrEmpty(fileUrl))
            {
                TempData["ErrorToast"] = "Không tìm thấy file CV.";
                return RedirectToAction("Applications");
            }

            if (fileUrl.StartsWith("http")) return Redirect(fileUrl);

            return File(fileUrl, "application/pdf");
        }
    }
}
