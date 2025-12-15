using IT_13FinalProject.Models;

namespace IT_13FinalProject.Services
{
    public class CurrentUserState
    {
        public int? UserId { get; private set; }
        public string? Username { get; private set; }
        public string? Role { get; private set; }
        public string? Email { get; private set; }
        public string? FullName { get; private set; }

        public bool IsAuthenticated => UserId.HasValue && !string.IsNullOrWhiteSpace(Username);

        public void Set(User user)
        {
            UserId = user.Id;
            Username = user.Username;
            Role = user.Role;
            Email = user.Email;
            FullName = user.FullName;
        }

        public void Clear()
        {
            UserId = null;
            Username = null;
            Role = null;
            Email = null;
            FullName = null;
        }
    }
}
