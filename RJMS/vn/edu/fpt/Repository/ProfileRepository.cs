using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        public ProfileRepository() { }

        public async Task<UserProfileDTO?> GetProfileByIdAsync(Guid userId)
        {
            // TODO: replace with database access once available
            await Task.CompletedTask;

            if (userId == Guid.Empty)
            {
                return new UserProfileDTO
                {
                    Id = Guid.NewGuid(),
                    FullName = "Sample User",
                    Email = "sample.user@example.com",
                    PhoneNumber = "+84 000 000 000",
                    AvatarUrl = string.Empty,
                    LastUpdatedAt = DateTime.UtcNow,
                };
            }

            return new UserProfileDTO
            {
                Id = userId,
                FullName = "Placeholder Name",
                Email = "placeholder@example.com",
                PhoneNumber = "+84 111 111 111",
                AvatarUrl = string.Empty,
                LastUpdatedAt = DateTime.UtcNow,
            };
        }
    }
}
