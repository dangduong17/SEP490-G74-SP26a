using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _repo;

        public AdminService(IAdminRepository repo)
        {
            _repo = repo;
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

            return new AdminUpdateUserViewModel
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
                RecruiterPosition = recruiter?.Position
            };
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

            var company = new Company
            {
                Name = model.CompanyName,
                TaxCode = model.CompanyTaxCode,
                CompanySize = model.CompanySize,
                Industry = model.CompanyIndustry,
                Website = model.CompanyWebsite,
                Email = model.CompanyEmail,
                Phone = model.CompanyPhone ?? model.PhoneNumber,
                Description = model.CompanyDescription,
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
                    await _repo.AddRecruiterAsync(recruiter);
                }
                else
                {
                    recruiter.FullName = $"{model.FirstName} {model.LastName}".Trim();
                    recruiter.Phone = model.PhoneNumber;
                    recruiter.Position = model.RecruiterPosition;
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
