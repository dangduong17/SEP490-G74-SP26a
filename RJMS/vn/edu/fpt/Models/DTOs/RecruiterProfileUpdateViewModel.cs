using System.ComponentModel.DataAnnotations;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    /// <summary>
    /// ViewModel dùng cho form chỉnh sửa hồ sơ nhà tuyển dụng.
    /// Bao gồm thông tin cá nhân (Recruiter) và thông tin công ty (Company).
    /// </summary>
    public class RecruiterProfileUpdateViewModel
    {
        // ── Hidden fields ─────────────────────────────────────────────────────
        public int RecruiterId { get; set; }
        public int? CompanyId { get; set; }

        // ── Thông tin cá nhân ─────────────────────────────────────────────────
        [Required(ErrorMessage = "Họ là bắt buộc.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vị trí công việc là bắt buộc.")]
        [MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        // ── Thông tin công ty ─────────────────────────────────────────────────
        [Required(ErrorMessage = "Tên công ty là bắt buộc.")]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CompanyTaxCode { get; set; }

        [MaxLength(50)]
        public string? CompanySize { get; set; }

        [MaxLength(200)]
        public string? CompanyIndustry { get; set; }

        [Url(ErrorMessage = "URL website không hợp lệ.")]
        [MaxLength(500)]
        public string? CompanyWebsite { get; set; }

        [EmailAddress(ErrorMessage = "Email công ty không hợp lệ.")]
        [MaxLength(100)]
        public string? CompanyEmail { get; set; }

        [Phone(ErrorMessage = "Số điện thoại công ty không hợp lệ.")]
        [MaxLength(20)]
        public string? CompanyPhone { get; set; }

        public string? CompanyDescription { get; set; }

        // ── Địa chỉ làm việc (Province / Ward lookup) ─────────────────────────
        public int? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public int? WardCode { get; set; }
        public string? WardName { get; set; }

        [MaxLength(500)]
        public string? WorkAddress { get; set; }
    }
}
