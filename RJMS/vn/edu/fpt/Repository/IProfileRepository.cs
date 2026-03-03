using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IProfileRepository
    {
        Task<UserProfileDTO?> GetProfileByIdAsync(Guid userId);
    }
}
