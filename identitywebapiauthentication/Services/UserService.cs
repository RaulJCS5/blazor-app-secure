using identitywebapiauthentication.Model;
using Microsoft.AspNetCore.Identity;

namespace identitywebapiauthentication.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRoleService _roleService;
        public UserService(UserManager<IdentityUser> userManager, IRoleService roleService)
        {
            _userManager = userManager;
            _roleService = roleService;
        }
        public async Task<bool> DeleteUserByEmail(string emailId)
        {
            var user = await _userManager.FindByEmailAsync(emailId);
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<List<UserModel>> GetAllUsers()
        {
            var response = new List<UserModel>();
            var users = _userManager.Users.ToList();
            foreach (var x in users)
            {
                var userRoles = await _userManager.GetRolesAsync(x);
                var user = new UserModel
                {
                    Id = new Guid(x.Id),
                    Email = x.Email,
                    UserName = x.UserName,
                    PhoneNumber = x.PhoneNumber,
                    Roles = userRoles.ToList()
                };
                response.Add(user);
            }
            return response;
        }

        public async Task<UserModel> GetUserById(string emailId)
        {
            var user = await _userManager.FindByEmailAsync(emailId);
            if (user == null)
            {
                // Handle the case where the user is not found
                return null; // or throw an exception, or return a default UserModel, depending on your requirements
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

        public async Task<bool> UpdateUser(string emailId, UserModel user)
        {
            //
            // user role - admin, hr
            var userIdentity = await _userManager.FindByEmailAsync(emailId);
            if (userIdentity == null)
            {
                return false;
            }
            userIdentity.UserName = user.UserName;
            userIdentity.Email = user.Email;
            userIdentity.PhoneNumber = user.PhoneNumber;

            var updatetResponse = await _userManager.UpdateAsync(userIdentity);
            if (updatetResponse.Succeeded)
            {
                // admin, user
                var userRoles = await _userManager.GetRolesAsync(userIdentity);
                // user role - admin, hr
                var removeUserRole = userRoles.Except(user.Roles);
                var removeRoleResult = await _userManager.RemoveFromRolesAsync(userIdentity, removeUserRole);
                if (removeRoleResult.Succeeded)
                {
                    var uniqueRole = user.Roles.Except(userRoles);
                    var assignRoleResult = await _roleService.AddUserRoleAsync(userIdentity.Email, uniqueRole.ToArray());
                    return assignRoleResult;
                }
            }
            return false;
        }
    }
}
