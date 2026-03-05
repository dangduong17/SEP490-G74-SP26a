using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RJMS.Vn.Edu.Fpt.Model.DTOs;
using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly FindingJobsDbContext _context;

        public JobApplicationRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<JobApplicationDTO>> GetApplicationsAsync(
            string userId
        )
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Array.Empty<JobApplicationDTO>();
            }

            if (!int.TryParse(userId, out var userIdValue))
            {
                return Array.Empty<JobApplicationDTO>();
            }

            var candidateId = await _context
                .Candidates.Where(c => c.UserId == userIdValue)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (candidateId == 0)
            {
                return Array.Empty<JobApplicationDTO>();
            }

            var applications = await _context
                .Applications.Where(a => a.CandidateId == candidateId)
                .Include(a => a.Job)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new JobApplicationDTO
                {
                    Id = a.Id,
                    PositionTitle = a.Job.Title ?? string.Empty,
                    Status = a.Status ?? string.Empty,
                    AppliedAt = a.CreatedAt ?? DateTime.MinValue,
                })
                .ToListAsync();

            return applications;
        }
    }
}
