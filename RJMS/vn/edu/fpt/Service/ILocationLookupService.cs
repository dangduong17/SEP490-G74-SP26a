using RJMS.vn.edu.fpt.Models.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface ILocationLookupService
    {
        Task<List<ProvinceLookupDto>> GetProvincesAsync();
        Task<List<WardLookupDto>> GetWardsByProvinceCodeAsync(int provinceCode);
    }
}
