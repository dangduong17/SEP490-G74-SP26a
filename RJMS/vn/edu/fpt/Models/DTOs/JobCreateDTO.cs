using System.ComponentModel.DataAnnotations;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    // DTO for Create/Edit Job
    public class JobSaveDTO
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề công việc")]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngành nghề")]
        public int JobCategoryId { get; set; }

        // Hierarchy fields for UI
        public int? JobCategoryLevel1Id { get; set; }
        public int? JobCategoryLevel2Id { get; set; }
        public int? JobCategoryLevel3Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức làm việc")]
        public string JobType { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "Số lượng tuyển phải lớn hơn 0")]
        public int? NumberOfPositions { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }

        // Location Info
        public int? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
        public int? WardCode { get; set; }
        public string? WardName { get; set; }
        public string? Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả công việc")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập yêu cầu ứng viên")]
        public string Requirements { get; set; } = string.Empty;

        public string? Benefits { get; set; }

        // Skills
        public List<int>? SelectedSkillIds { get; set; }
        public string? NewSkills { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn nộp hồ sơ")]
        public DateTime ApplicationDeadline { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày hết hạn tin")]
        public DateTime ExpiryDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày hiển thị")]
        public DateTime PublishDate { get; set; }

        public string? Status { get; set; }

        public string? ActionType { get; set; } // "Submit" or "Draft"
    }

    // DTO for Job List Item
    public class JobListItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? LocationName { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Status { get; set; }
        public int ApplicationCount { get; set; }
    }
}
