using System.Threading.Tasks;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IChatRepository
    {
        // Add database related methods here later
        Task<bool> PingAsync();
    }
}
