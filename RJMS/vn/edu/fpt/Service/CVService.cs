using RJMS.vn.edu.fpt.Models;
using RJMS.vn.edu.fpt.Models.DTOs;
using RJMS.Vn.Edu.Fpt.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Service
{
    public class CVService : ICVService
    {
        private readonly ICVRepository _cvRepository;

        public CVService(ICVRepository cvRepository)
        {
            _cvRepository = cvRepository;
        }

        public async Task<CandidateCvViewModel> GetCandidateCvsAsync(int userId)
        {
            var candidate = await _cvRepository.GetCandidateByUserIdAsync(userId);
            if (candidate == null)
            {
                return new CandidateCvViewModel();
            }

            var rawCvs = await _cvRepository.GetCvsByCandidateIdAsync(candidate.Id);

            // Mock Data that isn't directly in DB
            var viewModel = new CandidateCvViewModel
            {
                Cvs = rawCvs.Select((cv, index) => new CvListItemViewModel
                {
                    Id = cv.Id,
                    Title = string.IsNullOrWhiteSpace(cv.Title) ? "CV chưa đặt tên" : cv.Title,
                    UpdatedAt = cv.CreatedAt,
                    IsDefault = index == 0, // Mock latest as default
                    CandidateName = candidate.FullName ?? "Chưa cập nhật tên",
                    Position = candidate.Title ?? "Chưa cập nhật vị trí",
                    Experience = candidate.YearsOfExperience.HasValue ? $"{candidate.YearsOfExperience} năm kinh nghiệm" : "Chưa cập nhật kinh nghiệm",
                    Skills = new List<string> { "React", ".NET", "SQL" }, // Mock Skills as DB doesn't have it direct map on CV
                    FilePath = cv.FilePath
                }).ToList()
            };

            return viewModel;
        }
    }
}
