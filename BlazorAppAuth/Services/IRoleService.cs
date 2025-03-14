using BlazorAppAuth.Model;

namespace BlazorAppAuth.Services
{
    public interface IRoleService
    {
        Task<List<RoleModel>> GetAllRolesAsync(); // Renamed for clarity
        Task<List<string>> GetUserRolesAsync(string userEmail);
        Task<List<string>> AddRolesAsync(string[] roleNames);
        Task<bool> AssignRolesToUserAsync(string userEmail, string[] roleNames);
        Task<bool> DoesRoleExistAsync(string roleName); // Renamed for consistency
        Task<bool> CreateRoleAsync(string roleName);
        Task<bool> AssignRoleToUserAsync(string userEmail, string roleName);
        Task<bool> RenameRoleAsync(string currentRoleName, string newRoleName); // Renamed for better clarity
        Task<bool> UpdateUserRoleAsync(string userEmail, string currentRoleName, string newRoleName);
        Task<bool> DeleteRoleAsync(string roleName); // Renamed for clarity
        Task<bool> RevokeUserRoleAsync(string userEmail, string roleName); // Renamed for clarity
    }
}
