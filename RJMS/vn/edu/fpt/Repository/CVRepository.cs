using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class CVRepository : ICVRepository
    {
        private readonly FindingJobsDbContext _context;

        public CVRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cv>> GetCvsByCandidateIdAsync(int candidateId)
        {
            return await _context.Cvs
                .Where(c => c.CandidateId == candidateId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Candidate?> GetCandidateByUserIdAsync(int userId)
        {
            return await _context.Candidates
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
