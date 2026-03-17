using RJMS.vn.edu.fpt.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface ICVRepository
    {
        Task<List<Cv>> GetCvsByCandidateIdAsync(int candidateId);
        Task<Candidate?> GetCandidateByUserIdAsync(int userId);
    }
}
