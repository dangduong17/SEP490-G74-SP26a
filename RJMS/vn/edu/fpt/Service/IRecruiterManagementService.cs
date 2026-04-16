using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IRecruiterManagementService
    {
        // Company Locations
        Task<CompanyLocationsPageViewModel?> GetCompanyLocationsAsync(int userId);
        Task<(bool ok, string? error)> AddCompanyLocationAsync(int userId, RecruiterAddLocationViewModel model);
        Task<(bool ok, string? error)> DeleteCompanyLocationAsync(int userId, int companyLocationId);
        Task<(bool ok, string? error)> SetPrimaryLocationAsync(int userId, int companyLocationId);

        // Employees
        Task<RecruiterEmployeeListViewModel?> GetEmployeeListAsync(int userId, string? keyword, int page, int pageSize);
        Task<(bool ok, string? error)> CreateEmployeeAsync(int userId, RecruiterCreateEmployeeViewModel model);
        Task<RecruiterEditEmployeeViewModel?> GetEmployeeForEditAsync(int userId, int employeeId);
        Task<(bool ok, string? error)> UpdateEmployeeAsync(int userId, RecruiterEditEmployeeViewModel model);
        Task<(bool ok, string? error)> BanEmployeeAsync(int userId, int employeeId);
        Task<(bool ok, string? error)> UnbanEmployeeAsync(int userId, int employeeId);
    }
}
