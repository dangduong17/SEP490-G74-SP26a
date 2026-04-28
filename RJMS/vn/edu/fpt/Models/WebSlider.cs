using System;

namespace RJMS.vn.edu.fpt.Models;

public class WebSlider
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? LinkUrl { get; set; }
    public string? ButtonText { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
