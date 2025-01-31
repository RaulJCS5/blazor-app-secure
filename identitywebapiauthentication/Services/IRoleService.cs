using identitywebapiauthentication.Model;

namespace identitywebapiauthentication.Services
{
    public interface IRoleService
    {
        Task<List<RoleModel>> GetRolesAsync();
        Task<List<string>> GetUserRolesAsync(string emailId);
        Task<List<string>> AddRolesAsync(string[] roles);
        Task<bool> AddUserRoleAsync(string emailId, string[] role);
        Task<bool> RoleExistsAsync(string roleName); // New method
        Task<bool> CreateRoleAsync(string roleName); // New method
        Task<bool> AssignUserRoleAsync(string email, string roleName); // New method
        Task<bool> UpdateRoleAsync(string roleName, string newRoleName); // New method
        Task<bool> UpdateUserRoleAsync(string email, string currentRoleName, string newRoleName); // New method
        Task<bool> RemoveRoleAsync(string roleName); // New method
        Task<bool> RemoveUserRoleAsync(string email, string roleName); // New method
    }
}
