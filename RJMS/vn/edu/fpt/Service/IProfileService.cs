using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public interface IProfileService
    {
        Task<UserProfileDTO?> GetPersonalProfileAsync(Guid userId);
    }
}
