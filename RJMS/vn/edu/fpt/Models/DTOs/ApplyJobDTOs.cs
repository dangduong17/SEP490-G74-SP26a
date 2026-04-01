using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    // DTO để hiển thị CV khi chọn trong Apply Modal
    public class CvSelectItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CvType { get; set; } = "UPLOAD";
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsDefault { get; set; }
    }

    // Response trả về khi apply job
    public class ApplyJobResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // DTO để hiển thị thông báo
    public class NotificationDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public bool IsRead { get; set; }
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string TimeAgo => CreatedAt.HasValue ? GetTimeAgo(CreatedAt.Value) : "";

        private static string GetTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
            return dt.ToString("dd/MM/yyyy");
        }
    }

    // Danh sách CV dùng trong Apply modal
    public class ApplyModalData
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public List<CvSelectItemDto> Cvs { get; set; } = new();
        public int UploadCount { get; set; }
        public bool CanUploadMore => UploadCount < 3;
        public bool AlreadyApplied { get; set; }
    }
}
