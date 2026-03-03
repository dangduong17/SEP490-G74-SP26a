using System;

namespace RJMS.Vn.Edu.Fpt.Model.DTOs
{
    public class UserProfileDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime? LastUpdatedAt { get; set; }
    }
}
