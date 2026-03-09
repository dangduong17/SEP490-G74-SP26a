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

        public async Task<bool> AssignFreeSubscriptionIfRecruiterAsync(string email)
        {
            try
            {
                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return false;
                }

                // Check if user is a Recruiter
                var userRole = await _context.UserRoles
                    .Include(ur => ur.Role)
                    .FirstOrDefaultAsync(ur => ur.UserId == user.Id);

                if (userRole?.Role?.Name != "Recruiter")
                {
                    return false; // Not a recruiter, no subscription needed
                }

                // Check if user already has a subscription
                var existingSubscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (existingSubscription != null)
                {
                    return false; // Already has a subscription
                }

                // Find the free plan
                var freePlan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(sp => sp.Name == "Gói Miễn Phí" && sp.IsActive == true);

                if (freePlan == null)
                {
                    return false; // Free plan not found
                }

                // Create subscription
                var startDate = DateTime.UtcNow;
                var endDate = startDate.AddDays(freePlan.DurationDays ?? 30);

                var subscription = new Subscription
                {
                    UserId = user.Id,
                    PlanId = freePlan.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = "Active",
                    CreatedAt = startDate,
                    AutoRenew = false
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
