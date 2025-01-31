using identitywebapiauthentication.Model;
using Microsoft.AspNetCore.Identity;

namespace identitywebapiauthentication.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager){
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<List<string>> AddRolesAsync(string[] roles)
        {
            List<string> result = new List<string>();
            foreach (string role in roles)
            {
                bool roleExist = await _roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    IdentityRole newRole = new IdentityRole(role);
                    IdentityResult roleResult = await _roleManager.CreateAsync(newRole);
                    if (roleResult.Succeeded)
                    {
                        result.Add(role);
                    }
                }
            }
            return result;
        }

        public async Task<bool> AddUserRoleAsync(string emailId, string[] roles)
        {
            IdentityUser? user = await _userManager.FindByEmailAsync(emailId);
            List<string> exisitingRoles = await ExistsRolesAsync(roles);
            if (user != null && exisitingRoles.Count == roles.Length)
            {
                IdentityResult result = await _userManager.AddToRolesAsync(user, exisitingRoles);
                return result.Succeeded;
            }
            return false;
        }

        public async Task<List<RoleModel>> GetRolesAsync()
        {
            List<RoleModel> roles = _roleManager.Roles.Select(x => new RoleModel
            {
                Id = Guid.Parse(x.Id),
                Name = x.Name
            }).ToList();
            return roles;
        }

        private async Task<List<string>> ExistsRolesAsync(string[] roles)
        {
            List<string> result = new List<string>();
            foreach (string role in roles)
            {
                bool roleExist = await _roleManager.RoleExistsAsync(role);
                if (roleExist)
                {
                    result.Add(role);
                }
            }
            return result;
        }

        public async Task<List<string>> GetUserRolesAsync(string emailId)
        {
            IdentityUser? user = await _userManager.FindByEmailAsync(emailId);
            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            return userRoles.ToList();
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            return role != null;
        }

        public Task<bool> CreateRoleAsync(string roleName)
        {
            var role = new IdentityRole(roleName);
            return _roleManager.CreateAsync(role).ContinueWith(t => t.Result.Succeeded);
        }

        public async Task<bool> AssignUserRoleAsync(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return false;
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> UpdateRoleAsync(string roleName, string newRoleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return false;
            }

            var newRoleExists = await _roleManager.RoleExistsAsync(newRoleName);
            if (newRoleExists)
            {
                return false;
            }

            role.Name = newRoleName;
            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded;
        }

        public async Task<bool> UpdateUserRoleAsync(string email, string currentRoleName, string newRoleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var currentRoleExists = await _roleManager.RoleExistsAsync(currentRoleName);
            if (!currentRoleExists)
            {
                return false;
            }

            var roleExists = await _roleManager.RoleExistsAsync(newRoleName);
            if (!roleExists)
            {
                return false;
            }

            var isInRole = await _userManager.IsInRoleAsync(user, currentRoleName);
            if (!isInRole)
            {
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

        public async Task<bool> RemoveRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return false;
            }

            // Get all users in the role
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            foreach (var user in usersInRole)
            {
                // Remove the role from the user
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (!result.Succeeded)
                {
                    return false;
                }
            }

            // Delete the role
            var deleteResult = await _roleManager.DeleteAsync(role);
            return deleteResult.Succeeded;
        }

        public async Task<bool> RemoveUserRoleAsync(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return false;
            }

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!isInRole)
            {
                return false;
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }
    }
}
