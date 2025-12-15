using IT_13FinalProject.Models;

namespace IT_13FinalProject.Services
{
    public interface IUserAccountService
    {
        Task<bool> UserExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<List<User>> GetAllUsersAsync();
        Task<User?> UpdateUserAsync(User user);
        Task<User?> ChangeUsernameAsync(int userId, string newUsername);
        Task<User?> ChangePasswordAsync(int userId, string newPassword);
        Task<bool> DeleteUserAsync(int userId);

        // Legacy methods for compatibility with existing components
        bool UserExists(string userName);
        void AddUser(UserAccount user);
        UserAccount? ValidateUser(string userName, string password);
    }
}
