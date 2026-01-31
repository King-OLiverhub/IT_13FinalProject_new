using IT_13FinalProject.Data;
using IT_13FinalProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IT_13FinalProject.Services
{
    public class UserAccount
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    public class InMemoryUserAccountService : IUserAccountService
    {
        private readonly List<UserAccount> _users = new();

        public InMemoryUserAccountService()
        {
            _users.Add(new UserAccount { UserName = "Admin", Password = "12345", Role = "Admin" });
            _users.Add(new UserAccount { UserName = "Nurse", Password = "12345", Role = "Nurse" });
            _users.Add(new UserAccount { UserName = "Doctor", Password = "12345", Role = "Doctor" });
            _users.Add(new UserAccount { UserName = "Billing", Password = "12345", Role = "Billing Staff" });
            _users.Add(new UserAccount { UserName = "Inventory", Password = "12345", Role = "Inventory Staff" });
        }

        public bool UserExists(string userName)
        {
            return _users.Any(u => string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));
        }

        public void AddUser(UserAccount user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (UserExists(user.UserName))
            {
                throw new InvalidOperationException("User already exists.");
            }

            _users.Add(user);
        }

        public UserAccount? ValidateUser(string userName, string password)
        {
            return _users.FirstOrDefault(u =>
                string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        // Async methods - not implemented for in-memory
        public Task<bool> UserExistsAsync(string username) => Task.FromResult(UserExists(username));
        public Task<bool> EmailExistsAsync(string email) => Task.FromResult(false);
        public Task<User?> GetUserByUsernameAsync(string username) => Task.FromResult<User?>(null);
        public Task<User?> GetUserByEmailAsync(string email) => Task.FromResult<User?>(null);
        public Task<User> CreateUserAsync(User user) => throw new NotImplementedException("InMemory service doesn't support User model");
        public Task<User?> AuthenticateUserAsync(string username, string password) => Task.FromResult<User?>(null);
        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(new List<User>());
        public Task<User?> UpdateUserAsync(User user) => Task.FromResult<User?>(null);
        public Task<User?> ChangeUsernameAsync(int userId, string newUsername) => Task.FromResult<User?>(null);
        public Task<User?> ChangePasswordAsync(int userId, string newPassword) => Task.FromResult<User?>(null);
        public Task<bool> DeleteUserAsync(int userId) => Task.FromResult(false);
    }

    public class DatabaseUserAccountService : IUserAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly LocalDbContext _localContext;
        private readonly ILogger<DatabaseUserAccountService> _logger;
        private readonly SemaphoreSlim _dbLock = new(1, 1);

        public DatabaseUserAccountService(ApplicationDbContext context, LocalDbContext localContext, ILogger<DatabaseUserAccountService> logger)
        {
            _context = context; // Now using cloud database
            _localContext = localContext;
            _logger = logger;
        }

        private async Task<T> WithDbLock<T>(Func<Task<T>> action)
        {
            await _dbLock.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                _dbLock.Release();
            }
        }

        private async Task WithDbLock(Func<Task> action)
        {
            await _dbLock.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                _dbLock.Release();
            }
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await WithDbLock(async () =>
            {
                var normalized = username.ToLower();

                var existsInCloud = await _context.Users.AnyAsync(u => u.Username.ToLower() == normalized);
                if (existsInCloud) return true;

                return await _localContext.Users.AnyAsync(u => u.Username.ToLower() == normalized);
            });
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await WithDbLock(async () =>
            {
                var normalized = email.ToLower();

                var existsInCloud = await _context.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == normalized);
                if (existsInCloud) return true;

                return await _localContext.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == normalized);
            });
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await WithDbLock(async () =>
            {
                var normalized = username.ToLower();

                var cloudUser = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized);
                if (cloudUser != null) return cloudUser;

                return await _localContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized);
            });
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await WithDbLock(async () =>
            {
                var normalized = email.ToLower();

                var cloudUser = await _context.Users.FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalized);
                if (cloudUser != null) return cloudUser;

                return await _localContext.Users.FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == normalized);
            });
        }

        public async Task<User> CreateUserAsync(User user)
        {
            return await WithDbLock(async () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));

                var normalizedUsername = user.Username.ToLower();
                var normalizedEmail = user.Email?.ToLower();

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername) ||
                    await _localContext.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
                {
                    throw new InvalidOperationException("Username already exists.");
                }

                // Check if email already exists
                if (!string.IsNullOrEmpty(user.Email) &&
                    (await _context.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail) ||
                     await _localContext.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == normalizedEmail)))
                {
                    throw new InvalidOperationException("Email already exists.");
                }

                // Hash the password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                // Set defaults
                var createdAt = DateTime.UtcNow;

                var cloudUser = new User
                {
                    Username = user.Username,
                    Email = user.Email,
                    Password = hashedPassword,
                    Role = user.Role,
                    FullName = user.FullName,
                    CreatedAt = createdAt,
                    IsActive = true
                };

                var localUser = new User
                {
                    Username = user.Username,
                    Email = user.Email,
                    Password = hashedPassword,
                    Role = user.Role,
                    FullName = user.FullName,
                    CreatedAt = createdAt,
                    IsActive = true
                };

                // Save to cloud database first, then local.
                _context.Users.Add(cloudUser);
                await _context.SaveChangesAsync();

                try
                {
                    _localContext.Users.Add(localUser);
                    await _localContext.SaveChangesAsync();
                }
                catch (Exception localEx)
                {
                    // Best-effort compensation to avoid partial writes.
                    try
                    {
                        _context.Users.Remove(cloudUser);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "Failed to rollback cloud user after local save failed. Username: {Username}", cloudUser.Username);
                    }

                    _logger.LogError(localEx, "Failed to create user in local database. Username: {Username}", cloudUser.Username);
                    throw;
                }

                _logger.LogInformation("Created new user in cloud and local: {Username}", cloudUser.Username);
                return cloudUser;
            });
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            return await WithDbLock(async () =>
            {
                var normalized = username.ToLower();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized);
                var isFromCloud = user != null;

                if (user == null)
                {
                    user = await _localContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized);
                    isFromCloud = false;
                }

                if (user != null)
                {
                    var verified = false;
                    var needsUpgrade = false;

                    try
                    {
                        verified = BCrypt.Net.BCrypt.Verify(password, user.Password);
                    }
                    catch
                    {
                        // Stored password is not a valid BCrypt hash.
                        verified = string.Equals(user.Password, password, StringComparison.Ordinal);
                        needsUpgrade = verified;
                    }

                    if (verified)
                    {
                        if (needsUpgrade)
                        {
                            var newHash = BCrypt.Net.BCrypt.HashPassword(password);
                            try
                            {
                                if (isFromCloud)
                                {
                                    user.Password = newHash;
                                    await _context.SaveChangesAsync();

                                    // Best-effort upgrade local copy by username.
                                    var localUser = await _localContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized);
                                    if (localUser != null)
                                    {
                                        localUser.Password = newHash;
                                        await _localContext.SaveChangesAsync();
                                    }
                                }
                                else
                                {
                                    user.Password = newHash;
                                    await _localContext.SaveChangesAsync();
                                }
                            }
                            catch
                            {
                            }
                        }

                        return user;
                    }
                }

                return null;
            });
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await WithDbLock(async () =>
                await _context.Users.OrderBy(u => u.Username).ToListAsync());
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            return await WithDbLock(async () =>
            {
                if (user == null) throw new ArgumentNullException(nameof(user));

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                if (existingUser == null)
                {
                    throw new InvalidOperationException("User not found.");
                }

                // Update properties
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                existingUser.FullName = user.FullName;
                existingUser.IsActive = user.IsActive;

                if (!string.IsNullOrEmpty(user.Password) && user.Password != existingUser.Password)
                {
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                await _context.SaveChangesAsync();
                return existingUser;
            });
        }

        public async Task<User?> ChangeUsernameAsync(int userId, string newUsername)
        {
            return await WithDbLock(async () =>
            {
                if (string.IsNullOrWhiteSpace(newUsername))
                    throw new ArgumentException("Username is required.", nameof(newUsername));

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return null;

                var exists = await _context.Users.AnyAsync(u => u.Id != userId && u.Username.ToLower() == newUsername.ToLower());
                if (exists)
                    throw new InvalidOperationException("Username already exists.");

                user.Username = newUsername;
                await _context.SaveChangesAsync();
                return user;
            });
        }

        public async Task<User?> ChangePasswordAsync(int userId, string newPassword)
        {
            return await WithDbLock(async () =>
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                    throw new ArgumentException("Password is required.", nameof(newPassword));

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return null;

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();
                return user;
            });
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await WithDbLock(async () =>
            {
                // Delete from cloud (primary)
                var cloudUser = await _context.Users.FindAsync(userId);
                if (cloudUser == null) return false;

                _context.Users.Remove(cloudUser);
                await _context.SaveChangesAsync();

                // Best-effort delete from local by username to avoid needing matching IDs.
                var normalized = cloudUser.Username.ToLower();
                var localUser = await _localContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized);
                if (localUser != null)
                {
                    _localContext.Users.Remove(localUser);
                    await _localContext.SaveChangesAsync();
                }

                _logger.LogInformation("Deleted user: {Username}", cloudUser.Username);
                return true;
            });
        }

        // Legacy methods for compatibility with existing CreateAccount component
        public bool UserExists(string userName)
        {
            return UserExistsAsync(userName).GetAwaiter().GetResult();
        }

        public void AddUser(UserAccount userAccount)
        {
            var user = new User
            {
                Username = userAccount.UserName,
                Email = userAccount.Email ?? string.Empty,
                Password = userAccount.Password,
                Role = userAccount.Role,
                FullName = userAccount.Name,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            CreateUserAsync(user).GetAwaiter().GetResult();
        }

        public UserAccount? ValidateUser(string userName, string password)
        {
            var user = AuthenticateUserAsync(userName, password).GetAwaiter().GetResult();
            
            if (user == null) return null;

            return new UserAccount
            {
                UserName = user.Username,
                Email = user.Email,
                Role = user.Role,
                Name = user.FullName
            };
        }
    }
}
