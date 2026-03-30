using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _repo;
        private readonly ICloudinaryService _cloudinaryService;

        public AdminService(IAdminRepository repo, ICloudinaryService cloudinaryService)
        {
            _repo = repo;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var (total, active, inactive, admins, candidates, recruiters) =
                await _repo.GetDashboardStatsAsync();
            return new AdminDashboardViewModel
            {
                TotalUsers = total,
                ActiveUsers = active,
                InactiveUsers = inactive,
                TotalAdmins = admins,
                TotalCandidates = candidates,
                TotalRecruiters = recruiters
            };
        }

        public async Task<AdminUserListViewModel> GetUserListAsync(
            string? keyword, string? role, string? status, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            if (pageSize != 10 && pageSize != 20 && pageSize != 50) pageSize = 10;

            var (total, users) = await _repo.GetUsersPagedAsync(keyword, role, status, page, pageSize);

            var items = users.Select(u =>
            {
                var userRole = u.UserRoles.FirstOrDefault()?.Role?.Name ?? "N/A";
                var phone = u.Phone
                    ?? u.Candidates.FirstOrDefault()?.Phone
                    ?? u.Recruiters.FirstOrDefault()?.Phone;
                return new AdminUserListItemViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    PhoneNumber = phone,
                    Role = userRole,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive == true
                };
            }).ToList();

            return new AdminUserListViewModel
            {
                Keyword = keyword,
                Role = role,
                Status = status,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Users = items
            };
        }

        public async Task<AdminUpdateUserViewModel?> GetUpdateUserAsync(int id)
        {
            var user = await _repo.GetUserByIdWithDetailsAsync(id);
            if (user == null) return null;

            var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "Candidate";
            var candidate = user.Candidates.FirstOrDefault();
            var recruiter = user.Recruiters.FirstOrDefault();

            var model = new AdminUpdateUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.Phone ?? candidate?.Phone ?? recruiter?.Phone,
                Role = roleName,
                IsActive = user.IsActive == true,
                CandidateTitle = candidate?.Title,
                CandidateCity = candidate?.City,
                CandidateDateOfBirth = candidate?.DateOfBirth,
                CandidateGender = candidate?.Gender,
                CandidateAddress = candidate?.Address,
                CandidateCurrentSalary = candidate?.CurrentSalary,
                CandidateExpectedSalary = candidate?.ExpectedSalary,
                CandidateYearsOfExperience = candidate?.YearsOfExperience,
                CandidateSummary = candidate?.Summary,
                CandidateIsLookingForJob = candidate?.IsLookingForJob ?? false,
                RecruiterPosition = recruiter?.Position,
                RecruiterCompanyId = recruiter?.CompanyId
            };

            // Always load companies for dropdown (in case role is switched to Recruiter)
            var companies = await _repo.GetAllCompaniesAsync();
            model.Companies = companies
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            return model;
        }

        public async Task<ServiceResult> CreateAdminAsync(AdminCreateAdminViewModel model)
        {
            var pwd = ValidatePassword(model.Password);
            if (!pwd.Succeeded) return pwd;

            if (await _repo.UserEmailExistsAsync(model.Email))
                return ServiceResult.Failed(new ServiceError { Key = "Email", Message = "Email đã tồn tại." });

            var user = BuildUser(model.Email, model.Password, model.FirstName, model.LastName, model.PhoneNumber);
            await _repo.CreateUserAsync(user);
            await AssignRoleAsync(user.Id, "Admin");
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> CreateCandidateAsync(AdminCreateCandidateViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.PhoneNumber), Message = "Số điện thoại là bắt buộc." });
            if (string.IsNullOrWhiteSpace(model.Gender))
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.Gender), Message = "Giới tính là bắt buộc." });
            if (!model.DateOfBirth.HasValue)
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.DateOfBirth), Message = "Ngày sinh là bắt buộc." });

            var pwd = ValidatePassword(model.Password);
            if (!pwd.Succeeded) return pwd;

            if (await _repo.UserEmailExistsAsync(model.Email))
                return ServiceResult.Failed(new ServiceError { Key = "Email", Message = "Email đã tồn tại." });

            var user = BuildUser(model.Email, model.Password, model.FirstName, model.LastName);
            await _repo.CreateUserAsync(user);
            await AssignRoleAsync(user.Id, "Candidate");

            var candidate = new Candidate
            {
                UserId = user.Id,
                FullName = $"{model.FirstName} {model.LastName}".Trim(),
                Phone = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                IsLookingForJob = true,
                CreatedAt = DateTimeHelper.NowVietnam
            };
            await _repo.AddCandidateAsync(candidate);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> CreateRecruiterAsync(AdminCreateRecruiterViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.PhoneNumber), Message = "Số điện thoại là bắt buộc." });
            if (string.IsNullOrWhiteSpace(model.Position))
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.Position), Message = "Vị trí công việc là bắt buộc." });
            if (string.IsNullOrWhiteSpace(model.CompanyName))
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.CompanyName), Message = "Tên công ty là bắt buộc." });

            if (!string.IsNullOrWhiteSpace(model.CompanyTaxCode) &&
                await _repo.CompanyTaxCodeExistsAsync(model.CompanyTaxCode))
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.CompanyTaxCode), Message = "Mã số thuế đã tồn tại." });

            var pwd = ValidatePassword(model.Password);
            if (!pwd.Succeeded) return pwd;

            if (await _repo.UserEmailExistsAsync(model.Email))
                return ServiceResult.Failed(new ServiceError { Key = "Email", Message = "Email đã tồn tại." });

            var user = BuildUser(model.Email, model.Password, model.FirstName, model.LastName);
            await _repo.CreateUserAsync(user);
            await AssignRoleAsync(user.Id, "Recruiter");

            string? logoUrl = null;
            if (model.CompanyLogoFile != null)
            {
                logoUrl = await _cloudinaryService.UploadImageAsync(model.CompanyLogoFile, "logos");
            }

            var company = new Company
            {
                Name = model.CompanyName,
                Logo = logoUrl,
                TaxCode = model.CompanyTaxCode,
                CompanySize = model.CompanySize,
                Industry = model.CompanyIndustry,
                Website = model.CompanyWebsite,
                Email = model.CompanyEmail,
                Phone = model.CompanyPhone ?? model.PhoneNumber,
                Description = model.CompanyDescription,
                ProvinceCode = model.ProvinceCode,
                ProvinceName = model.ProvinceName,
                WardCode = model.WardCode,
                WardName = model.WardName,
                Address = model.WorkAddress,
                IsVerified = true,
                VerifiedAt = DateTimeHelper.NowVietnam,
                CreatedAt = DateTimeHelper.NowVietnam,
                UpdatedAt = DateTimeHelper.NowVietnam
            };
            await _repo.AddCompanyAsync(company);

            var recruiter = new Recruiter
            {
                UserId = user.Id,
                CompanyId = company.Id,
                FullName = $"{model.FirstName} {model.LastName}".Trim(),
                Phone = model.PhoneNumber,
                Position = model.Position,
                IsVerified = true,
                VerifiedAt = DateTimeHelper.NowVietnam,
                CreatedAt = DateTimeHelper.NowVietnam
            };
            await _repo.AddRecruiterAsync(recruiter);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> UpdateUserAsync(AdminUpdateUserViewModel model)
        {
            var supportedRoles = new[] { "Admin", "Candidate", "Recruiter" };
            if (!supportedRoles.Contains(model.Role))
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.Role), Message = "Vai trò không hợp lệ." });

            var user = await _repo.GetUserByIdWithDetailsAsync(model.Id);
            if (user == null) return ServiceResult.NotFoundResult();

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase) &&
                await _repo.UserEmailExistsAsync(model.Email, model.Id))
                return ServiceResult.Failed(new ServiceError { Key = nameof(model.Email), Message = "Email đã được sử dụng." });

            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Phone = model.PhoneNumber;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTimeHelper.NowVietnam;

            var currentUserRole = user.UserRoles.FirstOrDefault();
            if (currentUserRole == null || currentUserRole.Role?.Name != model.Role)
            {
                if (currentUserRole != null)
                    await _repo.RemoveUserRoleAsync(currentUserRole);
                await AssignRoleAsync(user.Id, model.Role);
            }

            await _repo.UpdateUserAsync(user);

            if (model.Role == "Candidate")
            {
                var candidate = user.Candidates.FirstOrDefault();
                if (candidate == null)
                {
                    candidate = new Candidate { UserId = user.Id, CreatedAt = DateTimeHelper.NowVietnam };
                    candidate.FullName = $"{model.FirstName} {model.LastName}".Trim();
                    candidate.Phone = model.PhoneNumber;
                    candidate.Title = model.CandidateTitle;
                    candidate.City = model.CandidateCity;
                    candidate.DateOfBirth = model.CandidateDateOfBirth;
                    candidate.Gender = model.CandidateGender;
                    candidate.Address = model.CandidateAddress;
                    candidate.CurrentSalary = model.CandidateCurrentSalary;
                    candidate.ExpectedSalary = model.CandidateExpectedSalary;
                    candidate.YearsOfExperience = model.CandidateYearsOfExperience;
                    candidate.Summary = model.CandidateSummary;
                    candidate.IsLookingForJob = model.CandidateIsLookingForJob;
                    await _repo.AddCandidateAsync(candidate);
                }
                else
                {
                    candidate.FullName = $"{model.FirstName} {model.LastName}".Trim();
                    candidate.Phone = model.PhoneNumber;
                    candidate.Title = model.CandidateTitle;
                    candidate.City = model.CandidateCity;
                    candidate.DateOfBirth = model.CandidateDateOfBirth;
                    candidate.Gender = model.CandidateGender;
                    candidate.Address = model.CandidateAddress;
                    candidate.CurrentSalary = model.CandidateCurrentSalary;
                    candidate.ExpectedSalary = model.CandidateExpectedSalary;
                    candidate.YearsOfExperience = model.CandidateYearsOfExperience;
                    candidate.Summary = model.CandidateSummary;
                    candidate.IsLookingForJob = model.CandidateIsLookingForJob;
                    await _repo.UpdateCandidateAsync(candidate);
                }
            }
            else if (model.Role == "Recruiter")
            {
                var recruiter = user.Recruiters.FirstOrDefault();
                if (recruiter == null)
                {
                    recruiter = new Recruiter { UserId = user.Id, CreatedAt = DateTimeHelper.NowVietnam };
                    recruiter.FullName = $"{model.FirstName} {model.LastName}".Trim();
                    recruiter.Phone = model.PhoneNumber;
                    recruiter.Position = model.RecruiterPosition;
                    recruiter.CompanyId = model.RecruiterCompanyId;
                    await _repo.AddRecruiterAsync(recruiter);
                }
                else
                {
                    recruiter.FullName = $"{model.FirstName} {model.LastName}".Trim();
                    recruiter.Phone = model.PhoneNumber;
                    recruiter.Position = model.RecruiterPosition;
                    recruiter.CompanyId = model.RecruiterCompanyId;
                    await _repo.UpdateRecruiterAsync(recruiter);
                }
            }

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> SoftDeleteUserAsync(int id)
        {
            var user = await _repo.GetUserByIdWithDetailsAsync(id);
            if (user == null) return ServiceResult.NotFoundResult();

            user.IsActive = false;
            user.UpdatedAt = DateTimeHelper.NowVietnam;
            await _repo.SoftDeleteUserAsync(user);
            return ServiceResult.Success();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static User BuildUser(string email, string password, string firstName, string lastName, string? phone = null)
        {
            return new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTimeHelper.NowVietnam,
                UpdatedAt = DateTimeHelper.NowVietnam
            };
        }

        private async Task AssignRoleAsync(int userId, string roleName)
        {
            var role = await _repo.GetRoleByNameAsync(roleName);
            if (role == null) return;
            await _repo.AddUserRoleAsync(new UserRole
            {
                UserId = userId,
                RoleId = role.Id,
                AssignedAt = DateTimeHelper.NowVietnam
            });
        }

        // ========== SKILLS MANAGEMENT ==========

        public async Task<AdminSkillListViewModel> GetSkillListAsync(string? keyword, string? category, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            if (pageSize != 10 && pageSize != 20 && pageSize != 50) pageSize = 20;

            var (total, skills) = await _repo.GetSkillsPagedAsync(keyword, category, page, pageSize);
            var categories = await _repo.GetSkillCategoriesAsync();

            var items = skills.Select(s => new AdminSkillListItemViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category,
                JobCount = s.JobSkills.Count
            }).ToList();

            return new AdminSkillListViewModel
            {
                Keyword = keyword,
                Category = category,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Skills = items,
                Categories = categories
            };
        }

        public async Task<AdminUpdateSkillViewModel?> GetSkillForEditAsync(int id)
        {
            var skill = await _repo.GetSkillByIdAsync(id);
            if (skill == null) return null;

            return new AdminUpdateSkillViewModel
            {
                Id = skill.Id,
                Name = skill.Name,
                Category = skill.Category
            };
        }

        public async Task<ServiceResult> CreateSkillAsync(AdminCreateSkillViewModel model)
        {
            // Validate
            if (await _repo.SkillNameExistsAsync(model.Name))
            {
                return ServiceResult.Failed(new ServiceError
                {
                    Key = "Name",
                    Message = $"Kỹ năng '{model.Name}' đã tồn tại."
                });
            }

            var skill = new Skill
            {
                Name = model.Name.Trim(),
                Category = string.IsNullOrWhiteSpace(model.Category) ? null : model.Category.Trim()
            };

            await _repo.AddSkillAsync(skill);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> UpdateSkillAsync(AdminUpdateSkillViewModel model)
        {
            var skill = await _repo.GetSkillByIdAsync(model.Id);
            if (skill == null) return ServiceResult.NotFoundResult();

            // Validate
            if (await _repo.SkillNameExistsAsync(model.Name, model.Id))
            {
                return ServiceResult.Failed(new ServiceError
                {
                    Key = "Name",
                    Message = $"Kỹ năng '{model.Name}' đã tồn tại."
                });
            }

            skill.Name = model.Name.Trim();
            skill.Category = string.IsNullOrWhiteSpace(model.Category) ? null : model.Category.Trim();

            await _repo.UpdateSkillAsync(skill);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteSkillAsync(int id)
        {
            var skill = await _repo.GetSkillByIdAsync(id);
            if (skill == null) return ServiceResult.NotFoundResult();

            await _repo.DeleteSkillAsync(skill);
            return ServiceResult.Success();
        }

        // ========== COMPANIES MANAGEMENT ==========

        public async Task<AdminCompanyListViewModel> GetCompanyListAsync(string? keyword, string? industry, string? verificationStatus, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            if (pageSize != 10 && pageSize != 20 && pageSize != 50) pageSize = 10;

            var (total, companies) = await _repo.GetCompaniesPagedAsync(keyword, industry, verificationStatus, page, pageSize);

            var items = companies.Select(c => new AdminCompanyListItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                TaxCode = c.TaxCode,
                Industry = c.Industry,
                CompanySize = c.CompanySize,
                Email = c.Email,
                Phone = c.Phone,
                IsVerified = c.IsVerified ?? false,
                RecruiterCount = c.Recruiters.Count,
                JobCount = c.Jobs.Count,
                FollowerCount = c.Followers.Count,
                CreatedAt = c.CreatedAt
            }).ToList();

            return new AdminCompanyListViewModel
            {
                Keyword = keyword,
                Industry = industry,
                VerificationStatus = verificationStatus,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Companies = items
            };
        }

        public async Task<AdminCompanyDetailViewModel?> GetCompanyDetailAsync(int id)
        {
            var company = await _repo.GetCompanyByIdWithDetailsAsync(id);
            if (company == null) return null;

            var recruiters = company.Recruiters.Select(r => new CompanyRecruiterViewModel
            {
                Id = r.Id,
                FullName = r.FullName ?? "",
                Email = r.User?.Email ?? "",
                Position = r.Position,
                Phone = r.Phone,
                IsVerified = r.IsVerified ?? false
            }).ToList();

            var jobs = company.Jobs.Select(j => new CompanyJobViewModel
            {
                Id = j.Id,
                Title = j.Title,
                Location = null, // TODO: Load location if needed
                Salary = j.MinSalary.HasValue || j.MaxSalary.HasValue
                    ? $"{(j.MinSalary.HasValue ? j.MinSalary.Value.ToString("N0") : "")}{(j.MinSalary.HasValue && j.MaxSalary.HasValue ? " - " : "")}{(j.MaxSalary.HasValue ? j.MaxSalary.Value.ToString("N0") : "")} VNĐ"
                    : null,
                CreatedAt = j.CreatedAt
            }).ToList();

            return new AdminCompanyDetailViewModel
            {
                Id = company.Id,
                Name = company.Name,
                Logo = company.Logo,
                TaxCode = company.TaxCode,
                CompanySize = company.CompanySize,
                Industry = company.Industry,
                Website = company.Website,
                Email = company.Email,
                Phone = company.Phone,
                Description = company.Description,
                Benefits = company.Benefits,
                ProvinceName = company.ProvinceName,
                WardName = company.WardName,
                Address = company.Address,
                IsVerified = company.IsVerified ?? false,
                VerifiedAt = company.VerifiedAt,
                CreatedAt = company.CreatedAt,
                Recruiters = recruiters,
                Jobs = jobs
            };
        }

        public async Task<ServiceResult> VerifyCompanyAsync(int id)
        {
            var company = await _repo.GetCompanyByIdWithDetailsAsync(id);
            if (company == null) return ServiceResult.NotFoundResult();

            company.IsVerified = true;
            company.VerifiedAt = DateTimeHelper.NowVietnam;
            company.UpdatedAt = DateTimeHelper.NowVietnam;

            await _repo.UpdateCompanyAsync(company);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> UnverifyCompanyAsync(int id)
        {
            var company = await _repo.GetCompanyByIdWithDetailsAsync(id);
            if (company == null) return ServiceResult.NotFoundResult();

            company.IsVerified = false;
            company.VerifiedAt = null;
            company.UpdatedAt = DateTimeHelper.NowVietnam;

            await _repo.UpdateCompanyAsync(company);
            return ServiceResult.Success();
        }

        // ========== SUBSCRIPTIONS MANAGEMENT ==========

        public async Task<AdminSubscriptionListViewModel> GetSubscriptionListAsync(string? keyword, string? status, int? planId, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            if (pageSize != 10 && pageSize != 20 && pageSize != 50) pageSize = 10;

            var (total, subscriptions) = await _repo.GetSubscriptionsPagedAsync(keyword, status, planId, page, pageSize);
            var plans = await _repo.GetActiveSubscriptionPlansAsync();

            var items = subscriptions.Select(s => new AdminSubscriptionListItemViewModel
            {
                Id = s.Id,
                UserId = s.UserId,
                UserEmail = s.User.Email,
                UserName = $"{s.User.FirstName} {s.User.LastName}".Trim(),
                PlanName = s.Plan?.Name ?? "N/A",
                PlanPrice = s.Plan?.Price ?? 0,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Status = s.Status ?? "N/A",
                AutoRenew = s.AutoRenew ?? false,
                CreatedAt = s.CreatedAt
            }).ToList();

            var planLookup = plans.Select(p => new SubscriptionPlanLookupViewModel
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Price = p.Price ?? 0
            }).ToList();

            return new AdminSubscriptionListViewModel
            {
                Keyword = keyword,
                Status = status,
                PlanId = planId,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Subscriptions = items,
                Plans = planLookup
            };
        }

        public async Task<AdminSubscriptionDetailViewModel?> GetSubscriptionDetailAsync(int id)
        {
            var subscription = await _repo.GetSubscriptionByIdWithDetailsAsync(id);
            if (subscription == null) return null;

            var userRole = subscription.User.UserRoles.FirstOrDefault()?.Role?.Name ?? "N/A";

            var payments = subscription.Payments.Select(p => new SubscriptionPaymentViewModel
            {
                Id = p.Id,
                Amount = p.Amount ?? 0,
                PaymentDate = p.PaymentDate,
                TransactionId = p.TransactionId,
                Status = p.Status ?? "N/A",
                PaymentMethod = p.PaymentMethod
            }).ToList();

            return new AdminSubscriptionDetailViewModel
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                UserEmail = subscription.User.Email,
                UserName = $"{subscription.User.FirstName} {subscription.User.LastName}".Trim(),
                UserRole = userRole,
                PlanId = subscription.PlanId,
                PlanName = subscription.Plan?.Name ?? "N/A",
                PlanPrice = subscription.Plan?.Price ?? 0,
                PlanDurationDays = subscription.Plan?.DurationDays ?? 0,
                PlanDescription = subscription.Plan?.Description,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                Status = subscription.Status ?? "N/A",
                AutoRenew = subscription.AutoRenew ?? false,
                CreatedAt = subscription.CreatedAt,
                Payments = payments
            };
        }

        // ========== PRIVATE HELPERS ==========

        private static ServiceResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8
                || !password.Any(char.IsUpper)
                || !password.Any(char.IsDigit)
                || password.All(char.IsLetterOrDigit))
            {
                return ServiceResult.Failed(new ServiceError
                {
                    Key = "Password",
                    Message = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ số và ký tự đặc biệt."
                });
            }
            return ServiceResult.Success();
        }
    }
}
