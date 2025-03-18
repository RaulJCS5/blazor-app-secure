using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApiAuth.Model;
using WebApiAuth.Repository;

namespace WebApiAuth.Service
{
    public class AuthService(IAuthRepository authRepository) : IAuthService
    {
        public Task<UserModel?> GetUserByLogin(string username, string password) =>
            authRepository.GetUserByLogin(username, HashPassword(password));
        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            await authRepository.RemoveRefreshTokenByUserID(refreshTokenModel.UserID);
            await authRepository.AddRefreshTokenModel(refreshTokenModel);
        }
        public Task<RefreshTokenModel?> GetRefreshTokenModel(string refreshToken) =>
            authRepository.GetRefreshTokenModel(refreshToken);

        public async Task<UserModel?> RegisterUser(RegisterModel registerModel)
        {
            var user = await authRepository.GetUser(registerModel.Username);
            if (user != null)
            {
                return null; // User already exists
            }

            var passwordHash = HashPassword(registerModel.Password);
            var newUser = new UserModel
            {
                Username = registerModel.Username,
                Password = passwordHash
            };

            bool userAdded = await authRepository.AddUser(newUser);
            if (!userAdded) return null; // Failed to add user

            // Assign roles
            foreach (var roleName in registerModel.Roles)
            {
                var role = await authRepository.GetRole(roleName);
                if (role != null)
                {
                    var newRole = new UserRoleModel
                    {
                        UserID = newUser.ID,
                        RoleID = role.ID
                    };
                    await authRepository.AddUserRole(newRole);
                }
            }

            return newUser;
        }
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }

        public Task<bool> AddRole(RoleModel role) => authRepository.AddRole(role);

        public async Task<bool> AssignRoleToUser(string username, string roleName)
        {
            var user = await authRepository.GetUser(username);
            var role = await authRepository.GetRole(roleName);

            if (user == null || role == null || await authRepository.UserRoleExists(username, roleName))
                return false; // User or Role does not exist, or Role already assigned

            return await authRepository.AddUserRole(new UserRoleModel { UserID = user.ID, RoleID = role.ID });
        }
    }
}