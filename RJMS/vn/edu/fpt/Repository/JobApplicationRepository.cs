using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RJMS.Models;
using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly G74FindingJobsContext _context;

        public JobApplicationRepository(G74FindingJobsContext context)
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

            var candidateId = await _context
                .Candidates.Where(c => c.UserId == userId)
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
                    Status = a.Status,
                    AppliedAt = a.CreatedAt,
                })
                .ToListAsync();

            return applications;
        }
    }
}
