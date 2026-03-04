using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RJMS.Models;
using RJMS.Vn.Edu.Fpt.Model.DTOs;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class CandidateDashboardRepository : ICandidateDashboardRepository
    {
        private readonly G74FindingJobsContext _context;

        public CandidateDashboardRepository(G74FindingJobsContext context)
        {
            _context = context;
        }

        public async Task<CandidateDashboardDTO> GetCandidateDashboardAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new CandidateDashboardDTO
                {
                    UserId = string.Empty,
                    TotalApplications = 0,
                    InterviewsScheduled = 0,
                    OffersReceived = 0,
                    LastUpdatedAt = DateTime.UtcNow,
                };
            }

            var candidateId = await _context
                .Candidates.Where(c => c.UserId == userId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (candidateId == 0)
            {
                return new CandidateDashboardDTO
                {
                    UserId = userId,
                    TotalApplications = 0,
                    InterviewsScheduled = 0,
                    OffersReceived = 0,
                    LastUpdatedAt = DateTime.UtcNow,
                };
            }

            var appQuery = _context
                .Applications.Where(a => a.CandidateId == candidateId)
                .AsNoTracking();

            var totalApplications = await appQuery.CountAsync();

            var interviewsScheduled = await appQuery
                .Where(a => a.Status == "Interview" || a.Status == "InterviewScheduled")
                .CountAsync();

            var offersReceived = await appQuery
                .Where(a => a.Status == "Offer" || a.Status == "Accepted")
                .CountAsync();

            return new CandidateDashboardDTO
            {
                UserId = userId,
                TotalApplications = totalApplications,
                InterviewsScheduled = interviewsScheduled,
                OffersReceived = offersReceived,
                LastUpdatedAt = DateTime.UtcNow,
            };
        }
    }
}
