using BCrypt.Net;
using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using RJMS.Vn.Edu.Fpt.Service;
using vn.edu.fpt.Utilities;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class RecruiterManagementService : IRecruiterManagementService
    {
        private readonly IRecruiterManagementRepository _repo;
        private readonly ILocationLookupService _locationLookup;

        public RecruiterManagementService(
            IRecruiterManagementRepository repo,
            ILocationLookupService locationLookup)
        {
            _repo = repo;
            _locationLookup = locationLookup;
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private async Task<(Recruiter? recruiter, Company? company)> ResolveAsync(int userId)
        {
            var recruiter = await _repo.GetRecruiterByUserIdAsync(userId);
            if (recruiter?.CompanyId == null) return (null, null);
            var company = await _repo.GetCompanyByIdAsync(recruiter.CompanyId.Value);
            return (recruiter, company);
        }

        private static bool ValidatePassword(string pwd, out string? error)
        {
            if (string.IsNullOrWhiteSpace(pwd) || pwd.Length < 8
                || !pwd.Any(char.IsUpper) || !pwd.Any(char.IsDigit)
                || pwd.All(char.IsLetterOrDigit))
            {
                error = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ số và ký tự đặc biệt.";
                return false;
            }
            error = null;
            return true;
        }

        private static string HashPassword(string raw)
        {
            return BCrypt.Net.BCrypt.HashPassword(raw);
        }

        private static string GetLocationDisplayText(CompanyLocation? companyLocation)
        {
            if (companyLocation == null)
            {
                return string.Empty;
            }

            var addressParts = new List<string>();

            var detailAddress = companyLocation.Location?.DetailAddress?.Trim();
            var streetAddress = companyLocation.Location?.Address?.Trim();
            var wardName = companyLocation.Location?.WardName?.Trim();
            var cityName = companyLocation.Location?.CityName?.Trim();
            var label = companyLocation.AddressLabel?.Trim();

            if (!string.IsNullOrWhiteSpace(detailAddress))
            {
                addressParts.Add(detailAddress);
            }
            else if (!string.IsNullOrWhiteSpace(streetAddress))
            {
                addressParts.Add(streetAddress);
            }
            else if (!string.IsNullOrWhiteSpace(label))
            {
                addressParts.Add(label);
            }

            if (!string.IsNullOrWhiteSpace(wardName))
            {
                addressParts.Add(wardName);
            }

            if (!string.IsNullOrWhiteSpace(cityName))
            {
                addressParts.Add(cityName);
            }

            return string.Join(", ", addressParts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        // ── Company Locations ──────────────────────────────────────────────

        public async Task<CompanyLocationsPageViewModel?> GetCompanyLocationsAsync(int userId)
        {
            var (recruiter, company) = await ResolveAsync(userId);
            if (company == null) return null;

            var cls = await _repo.GetCompanyLocationsAsync(company.Id);
            return new CompanyLocationsPageViewModel
            {
                CompanyId = company.Id,
                CompanyName = company.Name,
                Locations = cls.Select(cl => new RecruiterCompanyLocationViewModel
                {
                    Id = cl.Id,
                    LocationId = cl.LocationId,
                    AddressLabel = cl.AddressLabel,
                    IsPrimary = cl.IsPrimary,
                    CityName = cl.Location?.CityName ?? "",
                    WardName = cl.Location?.WardName,
                    Address = cl.Location?.Address,
                    ProvinceCode = cl.Location?.ProvinceCode,
                    WardCode = cl.Location?.WardCode,
                    EmployeeCount = cl.RecruiterLocations?.Count ?? 0,
                    CreatedAt = cl.CreatedAt
                }).ToList()
            };
        }

        public async Task<(bool ok, string? error)> AddCompanyLocationAsync(int userId, RecruiterAddLocationViewModel model)
        {
            var (recruiter, company) = await ResolveAsync(userId);
            if (company == null) return (false, "Không tìm thấy công ty.");

            // Resolve province/ward names if missing
            var provinceName = model.ProvinceName;
            var wardName = model.WardName;
            if (string.IsNullOrWhiteSpace(provinceName) && model.ProvinceCode.HasValue)
            {
                var provinces = await _locationLookup.GetProvincesAsync();
                provinceName = provinces.FirstOrDefault(p => p.Code == model.ProvinceCode)?.Name;
            }
            if (string.IsNullOrWhiteSpace(wardName) && model.WardCode.HasValue && model.ProvinceCode.HasValue)
            {
                var wards = await _locationLookup.GetWardsByProvinceCodeAsync(model.ProvinceCode.Value);
                wardName = wards.FirstOrDefault(w => w.Code == model.WardCode)?.Name;
            }

            var location = await _repo.GetMatchingLocationAsync(model.ProvinceCode, model.WardCode, model.WorkAddress);
            if (location == null)
            {
                location = new Location
                {
                    ProvinceCode = model.ProvinceCode,
                    CityName = provinceName ?? "Chưa rõ",
                    WardCode = model.WardCode,
                    WardName = wardName,
                    Address = model.WorkAddress
                };
                await _repo.AddLocationAsync(location);
            }

            // Unset other primaries if this is the new primary
            if (model.IsPrimary)
            {
                var existing = await _repo.GetCompanyLocationsAsync(company.Id);
                foreach (var ex in existing.Where(cl => cl.IsPrimary))
                {
                    ex.IsPrimary = false;
                }
                await _repo.SaveChangesAsync();
            }

            var cl = new CompanyLocation
            {
                CompanyId = company.Id,
                LocationId = location.Id,
                AddressLabel = model.AddressLabel,
                IsPrimary = model.IsPrimary,
                CreatedAt = DateTimeHelper.NowVietnam
            };
            await _repo.AddCompanyLocationAsync(cl);
            return (true, null);
        }

        public async Task<(bool ok, string? error)> DeleteCompanyLocationAsync(int userId, int companyLocationId)
        {
            var (_, company) = await ResolveAsync(userId);
            if (company == null) return (false, "Không tìm thấy công ty.");

            var cl = await _repo.GetCompanyLocationByIdAsync(companyLocationId);
            if (cl == null || cl.CompanyId != company.Id) return (false, "Không tìm thấy địa chỉ.");
            if (cl.RecruiterLocations?.Any() == true)
                return (false, "Không thể xóa địa chỉ đang có nhân viên được gán.");

            // Use SaveChanges via context - minimal approach
            return (false, "Chức năng xóa cần được gọi trực tiếp qua DbContext trong controller.");
        }

        public async Task<(bool ok, string? error)> SetPrimaryLocationAsync(int userId, int companyLocationId)
        {
            var (_, company) = await ResolveAsync(userId);
            if (company == null) return (false, "Không tìm thấy công ty.");

            var all = await _repo.GetCompanyLocationsAsync(company.Id);
            var target = all.FirstOrDefault(cl => cl.Id == companyLocationId);
            if (target == null || target.CompanyId != company.Id) return (false, "Không tìm thấy địa chỉ.");

            foreach (var cl in all) cl.IsPrimary = cl.Id == companyLocationId;
            await _repo.SaveChangesAsync();
            return (true, null);
        }

        // ── Employees ─────────────────────────────────────────────────────

        public async Task<RecruiterEmployeeListViewModel?> GetEmployeeListAsync(int userId, string? keyword, int page, int pageSize)
        {
            var (_, company) = await ResolveAsync(userId);
            if (company == null) return null;

            var (total, recruiters) = await _repo.GetEmployeesPagedAsync(company.Id, keyword, page, pageSize);
            var locations = await _repo.GetCompanyLocationsAsync(company.Id);

            var items = recruiters.Select(r => new RecruiterEmployeeItemViewModel
            {
                Id = r.Id,
                UserId = r.UserId,
                Email = r.User?.Email ?? "",
                FullName = r.FullName ?? "",
                Phone = r.Phone,
                Position = r.Position,
                Avatar = r.Avatar ?? r.User?.Avatar,
                IsActive = r.User?.IsActive ?? true,
                IsVerified = r.IsVerified ?? false,
                LocationLabels = r.RecruiterLocations?
                    .Select(rl => GetLocationDisplayText(rl.CompanyLocation))
                    .Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new(),
                CreatedAt = r.CreatedAt
            }).ToList();

            return new RecruiterEmployeeListViewModel
            {
                Employees = items,
                Keyword = keyword,
                TotalItems = total,
                Page = page,
                PageSize = pageSize,
                Locations = locations.Select(cl => new RecruiterCompanyLocationViewModel
                {
                    Id = cl.Id,
                    AddressLabel = cl.AddressLabel ?? "",
                    CityName = cl.Location?.CityName ?? "",
                    WardName = cl.Location?.WardName,
                    Address = GetLocationDisplayText(cl),
                    IsPrimary = cl.IsPrimary
                }).ToList()
            };
        }

        public async Task<(bool ok, string? error)> CreateEmployeeAsync(int userId, RecruiterCreateEmployeeViewModel model)
        {
            var (_, company) = await ResolveAsync(userId);
            if (company == null) return (false, "Không tìm thấy công ty.");

            if (string.IsNullOrWhiteSpace(model.Email))     return (false, "Email là bắt buộc.");
            if (string.IsNullOrWhiteSpace(model.PhoneNumber)) return (false, "Số điện thoại là bắt buộc.");
            if (string.IsNullOrWhiteSpace(model.Position))   return (false, "Vị trí công việc là bắt buộc.");
            if (!ValidatePassword(model.Password, out var pwdErr)) return (false, pwdErr);

            if (await _repo.UserEmailExistsAsync(model.Email))
                return (false, "Email này đã được sử dụng.");

            var user = new User
            {
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.PhoneNumber,
                IsActive = true,
                EmailConfirmed = false,
                CreatedAt = DateTimeHelper.NowVietnam
            };
            await _repo.CreateUserAsync(user);
            await _repo.AssignRoleAsync(user.Id, "Employee");

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

            var isFirst = true;
            foreach (var clId in model.CompanyLocationIds.Distinct())
            {
                await _repo.AddRecruiterLocationAsync(new RecruiterLocation
                {
                    RecruiterId = recruiter.Id,
                    CompanyLocationId = clId,
                    IsDefault = isFirst,
                    AssignedAt = DateTimeHelper.NowVietnam
                });
                isFirst = false;
            }

            return (true, null);
        }

        public async Task<RecruiterEditEmployeeViewModel?> GetEmployeeForEditAsync(int userId, int employeeId)
        {
            var (_, company) = await ResolveAsync(userId);
            if (company == null) return null;

            var emp = await _repo.GetEmployeeByIdAsync(employeeId, company.Id);
            if (emp == null) return null;

            var locations = await _repo.GetCompanyLocationsAsync(company.Id);
            var currentLocIds = emp.RecruiterLocations?.Select(rl => rl.CompanyLocationId).ToList() ?? new();

            return new RecruiterEditEmployeeViewModel
            {
                RecruiterId = emp.Id,
                FullName = emp.FullName ?? "",
                Phone = emp.Phone,
                Position = emp.Position,
                CompanyLocationIds = currentLocIds,
                CurrentLocationIds = currentLocIds,
                Locations = locations.Select(cl => new RecruiterCompanyLocationViewModel
                {
                    Id = cl.Id,
                    AddressLabel = cl.AddressLabel ?? "",
                    CityName = cl.Location?.CityName ?? "",
                    WardName = cl.Location?.WardName,
                    Address = GetLocationDisplayText(cl),
                    IsPrimary = cl.IsPrimary
                }).ToList()
            };
        }

        public async Task<(bool ok, string? error)> UpdateEmployeeAsync(int userId, RecruiterEditEmployeeViewModel model)
        {
            var (_, company) = await ResolveAsync(userId);
            if (company == null) return (false, "Không tìm thấy công ty.");

            var emp = await _repo.GetEmployeeByIdAsync(model.RecruiterId, company.Id);
            if (emp == null) return (false, "Không tìm thấy nhân viên.");

            emp.FullName = model.FullName;
            emp.Phone = model.Phone;
            emp.Position = model.Position;
            await _repo.UpdateRecruiterAsync(emp);

            // Re-assign locations
            await _repo.RemoveAllRecruiterLocationsAsync(emp.Id);
            var isFirst = true;
            foreach (var clId in model.CompanyLocationIds.Distinct())
            {
                await _repo.AddRecruiterLocationAsync(new RecruiterLocation
                {
                    RecruiterId = emp.Id,
                    CompanyLocationId = clId,
                    IsDefault = isFirst,
                    AssignedAt = DateTimeHelper.NowVietnam
                });
                isFirst = false;
            }
            return (true, null);
        }

        public async Task<(bool ok, string? error)> BanEmployeeAsync(int userId, int employeeId)
        {
            var (_, company) = await ResolveAsync(userId);
            if (company == null) return (false, "Không tìm thấy công ty.");

            var emp = await _repo.GetEmployeeByIdAsync(employeeId, company.Id);
            if (emp == null) return (false, "Không tìm thấy nhân viên.");

            await _repo.SetUserActiveAsync(emp.UserId, false);
            return (true, null);
        }

        public async Task<(bool ok, string? error)> UnbanEmployeeAsync(int userId, int employeeId)
        {
            var (_, company) = await ResolveAsync(userId);
            if (company == null) return (false, "Không tìm thấy công ty.");

            var emp = await _repo.GetEmployeeByIdAsync(employeeId, company.Id);
            if (emp == null) return (false, "Không tìm thấy nhân viên.");

            await _repo.SetUserActiveAsync(emp.UserId, true);
            return (true, null);
        }
    }
}
