namespace RJMS.Vn.Edu.Fpt.Repository
{
    public interface IAuthRepository
    {
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
        Task<object?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
    }
}
