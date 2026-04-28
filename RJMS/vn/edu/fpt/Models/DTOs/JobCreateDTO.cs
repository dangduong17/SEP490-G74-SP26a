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

        [Range(1, 50, ErrorMessage = "S? lu?ng tuy?n ph?i trong kho?ng t? 1 d?n 50")]
        public int? NumberOfPositions { get; set; }

        [Range(typeof(decimal), "0", "500000000", ErrorMessage = "M?c luong t?i thi?u ph?i trong kho?ng t? 0 d?n 500000000")]
        public decimal? MinSalary { get; set; }
        [Range(typeof(decimal), "0", "500000000", ErrorMessage = "M?c luong t?i da ph?i trong kho?ng t? 0 d?n 500000000")]
        public decimal? MaxSalary { get; set; }

        // Location Info
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một địa điểm làm việc")]
        public List<int>? SelectedCompanyLocationIds { get; set; } = new List<int>();

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
        [Range(typeof(decimal), "0", "500000000", ErrorMessage = "M?c luong t?i thi?u ph?i trong kho?ng t? 0 d?n 500000000")]
        public decimal? MinSalary { get; set; }
        [Range(typeof(decimal), "0", "500000000", ErrorMessage = "M?c luong t?i da ph?i trong kho?ng t? 0 d?n 500000000")]
        public decimal? MaxSalary { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Status { get; set; }
        public int ApplicationCount { get; set; }
    }
}

