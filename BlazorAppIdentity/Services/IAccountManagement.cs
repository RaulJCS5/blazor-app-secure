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
    }
}
