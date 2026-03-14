using RJMS.vn.edu.fpt.Models;
using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly FindingJobsDbContext _context;

        public ChatRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public Task<bool> PingAsync()
        {
            return Task.FromResult(true);
        }
    }
}
