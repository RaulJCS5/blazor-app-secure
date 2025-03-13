
using BlazorAppAuth.ViewModel;

namespace BlazorAppAuth.Services
{
    public interface IAccountManagement
    {
        public Task<bool> CheckAuthenticatedAsync();
        public Task<List<Role>> GetRolesAsync();
        public Task<FormResult> AddRoleAsync(string[] roles);
        public Task<UserViewModel[]> GetUsers();
        public Task<UserViewModel> GetUserByEmail(string userEmailId);
        public Task<bool> UserUpdate(string userEmailId, UserViewModel user);
        public Task<bool> UserDelete(string userEmailId);
    }
}
