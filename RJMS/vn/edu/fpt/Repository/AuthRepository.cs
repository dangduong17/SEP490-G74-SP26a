using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RJMS.vn.edu.fpt.Models;

namespace RJMS.Vn.Edu.Fpt.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly FindingJobsDbContext _context;

        public AuthRepository(FindingJobsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            try
            {
                // Use BCrypt to verify the password against the stored hash
                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch
            {
                // Fallback for plain text or old hashes if necessary during migration
                return user.PasswordHash == password;
            }
        }

        public async Task<object?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UpdatePasswordHashAsync(string email, string newPasswordHash)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = newPasswordHash;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> SetEmailConfirmedAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return false;
            }

            user.EmailConfirmed = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Candidate?> CreateCandidateAsync(Candidate candidate)
        {
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
            return candidate;
        }

        public async Task<string> GetUserRoleAsync(int userId)
        {
            var userRole = await _context.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == userId);
            
            return userRole?.Role?.Name ?? "Candidate";
        }
    }
}
