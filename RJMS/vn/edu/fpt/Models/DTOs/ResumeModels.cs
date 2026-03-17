using System;
using System.Collections.Generic;

namespace RJMS.vn.edu.fpt.Models.DTOs
{
    public class CvListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDefault { get; set; }
        public string CandidateName { get; set; }
        public string Position { get; set; }
        public string Experience { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public string FilePath { get; set; }
    }

    public class CandidateCvViewModel
    {
        public List<CvListItemViewModel> Cvs { get; set; } = new List<CvListItemViewModel>();
    }
}
