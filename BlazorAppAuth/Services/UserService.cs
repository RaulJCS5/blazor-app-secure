using BlazorAppAuth.Database;
using BlazorAppAuth.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorAppAuth.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRoleService _roleService;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<User> userManager, IRoleService roleService, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleService = roleService;
            _logger = logger;
        }
        public async Task<bool> DeleteUserAsync(string userEmail)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {userEmail}");
                    return false;
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError($"Failed to delete user: {userEmail}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user {userEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            try
            {
                var response = new List<UserModel>();
                var users = await _userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var userModel = new UserModel
                    {
                        Id = new Guid(user.Id),
                        Email = user.Email,
                        UserName = user.UserName,
                        PhoneNumber = user.PhoneNumber,
                        Roles = userRoles.ToList()
                    };
                    response.Add(userModel);
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all users: {ex.Message}");
                return new List<UserModel>();
            }
        }

        public async Task<UserModel> GetUserByEmailAsync(string userEmail)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {userEmail}");
                    return null;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userModel = new UserModel
                {
                    Id = new Guid(user.Id),
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = userRoles.ToList()
                };
                return userModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user by email {userEmail}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(string userEmail, UserModel user)
        {
            try
            {
                var userIdentity = await _userManager.FindByEmailAsync(userEmail);
                if (userIdentity == null)
                {
                    _logger.LogWarning($"User not found: {userEmail}");
                    return false;
                }

                userIdentity.UserName = user.UserName;
                userIdentity.Email = user.Email;
                userIdentity.PhoneNumber = user.PhoneNumber;

                var updateResponse = await _userManager.UpdateAsync(userIdentity);
                if (!updateResponse.Succeeded)
                {
                    _logger.LogError($"Failed to update user: {userEmail}");
                    return false;
                }

                // Remove roles that are no longer assigned
                var userRoles = await _userManager.GetRolesAsync(userIdentity);
                var rolesToRemove = userRoles.Except(user.Roles).ToList();
                var removeRoleResult = await _userManager.RemoveFromRolesAsync(userIdentity, rolesToRemove);

                if (!removeRoleResult.Succeeded)
                {
                    _logger.LogError($"Failed to remove roles for user: {userEmail}");
                    return false;
                }

                // Assign new roles that are not yet assigned
                var rolesToAdd = user.Roles.Except(userRoles).ToList();
                var assignRoleResult = await _roleService.AssignRolesToUserAsync(userIdentity.Email, rolesToAdd.ToArray());

                return assignRoleResult;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user {userEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
