using BlazorAppIdentity.Model;

namespace BlazorAppIdentity.Services
{
    public interface IAccountManagement
    {
        public Task<FormResult> RegisterAsync(string email, string password);
        public Task<FormResult> LoginAsync(string email, string password);
        public Task LogoutAsync();
        public Task<bool> CheckAuthenticatedAsync();
        public Task<List<Role>> GetRolesAsync();
        public Task<FormResult> AddRoleAsync(string[] roles);
        public Task<UserViewModel[]> GetUsers();
        public Task<UserViewModel> GetUserByEmail(string userEmailId);
    }
}
