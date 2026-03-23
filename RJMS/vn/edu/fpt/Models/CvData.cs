using System;

namespace RJMS.vn.edu.fpt.Models;

public class CvData
{
    public int Id { get; set; }
    public int CvId { get; set; }
    public string JsonData { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Cv Cv { get; set; } = null!;
}
