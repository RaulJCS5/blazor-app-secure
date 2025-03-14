using BlazorAppAuth.Database;
using BlazorAppAuth.Model;
using Microsoft.AspNetCore.Identity;

namespace BlazorAppAuth.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RoleService> _logger;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, ILogger<RoleService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<List<string>> AddRolesAsync(string[] roleNames)
        {
            var addedRoles = new List<string>();

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

                    if (result.Succeeded)
                    {
                        addedRoles.Add(roleName);
                    }
                    else
                    {
                        _logger.LogError($"Failed to create role: {roleName}");
                    }
                }
            }
            return addedRoles;
        }

        public async Task<bool> AssignRolesToUserAsync(string userEmail, string[] roleNames)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {userEmail}");
                    return false;
                }

                var validRoles = await FilterExistingRolesAsync(roleNames);
                if (!validRoles.Any())
                {
                    _logger.LogWarning("No valid roles found to assign.");
                    return false;
                }

                var result = await _userManager.AddToRolesAsync(user, validRoles);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error assigning roles to user {userEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<RoleModel>> GetAllRolesAsync()
        {
            try
            {
                return _roleManager.Roles
                    .Select(r => new RoleModel { Id = Guid.Parse(r.Id), Name = r.Name })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving roles: {ex.Message}");
                return new List<RoleModel>();
            }
        }

        private async Task<List<string>> FilterExistingRolesAsync(string[] roleNames)
        {
            var validRoles = new List<string>();

            foreach (var roleName in roleNames)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    validRoles.Add(roleName);
                }
            }
            return validRoles;
        }

        public async Task<List<string>> GetUserRolesAsync(string userEmail)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                return user == null ? new List<string>() : (await _userManager.GetRolesAsync(user)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving roles for user {userEmail}: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> DoesRoleExistAsync(string roleName)
        {
            try
            {
                return await _roleManager.RoleExistsAsync(roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking if role exists: {roleName}, {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateRoleAsync(string roleName)
        {
            try
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogWarning($"Role already exists: {roleName}");
                    return false;
                }

                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating role {roleName}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AssignRoleToUserAsync(string userEmail, string roleName)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null || !await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogWarning($"User or role not found: {userEmail}, {roleName}");
                    return false;
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error assigning role {roleName} to user {userEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RenameRoleAsync(string currentRoleName, string newRoleName)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(currentRoleName);
                if (role == null || await _roleManager.RoleExistsAsync(newRoleName))
                {
                    _logger.LogWarning($"Role rename failed: {currentRoleName} to {newRoleName}");
                    return false;
                }

                role.Name = newRoleName;
                var result = await _roleManager.UpdateAsync(role);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error renaming role {currentRoleName} to {newRoleName}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(string userEmail, string currentRoleName, string newRoleName)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null || !await _roleManager.RoleExistsAsync(currentRoleName) || !await _roleManager.RoleExistsAsync(newRoleName))
                {
                    _logger.LogWarning($"User or role not found: {userEmail}, {currentRoleName}, {newRoleName}");
                    return false;
                }

                if (!await _userManager.IsInRoleAsync(user, currentRoleName))
                {
                    _logger.LogWarning($"User {userEmail} is not in role {currentRoleName}");
                    return false;
                }

                var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRoleName);
                if (!removeResult.Succeeded)
                {
                    return false;
                }

                var addResult = await _userManager.AddToRoleAsync(user, newRoleName);
                return addResult.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user role {userEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteRoleAsync(string roleName)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                {
                    _logger.LogWarning($"Role not found: {roleName}");
                    return false;
                }

                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
                foreach (var user in usersInRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, roleName);
                }

                var result = await _roleManager.DeleteAsync(role);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting role {roleName}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RevokeUserRoleAsync(string userEmail, string roleName)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null || !await _roleManager.RoleExistsAsync(roleName) || !await _userManager.IsInRoleAsync(user, roleName))
                {
                    _logger.LogWarning($"User-role mismatch: {userEmail}, {roleName}");
                    return false;
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error revoking role {roleName} from user {userEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
