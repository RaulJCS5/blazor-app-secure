using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApiAuth.Model;
using WebApiAuth.Repository;

namespace WebApiAuth.Service
{
    public class AuthService(IAuthRepository authRepository) : IAuthService
    {
        public Task<UserModel> GetUserByLogin(string username, string password)
        {
            var passwordHash = HashPassword(password);
            return authRepository.GetUserByLogin(username, passwordHash);
        }
        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            await authRepository.RemoveRefreshTokenByUserID(refreshTokenModel.UserID);
            await authRepository.AddRefreshTokenModel(refreshTokenModel);
        }
        public Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken)
        {
            return authRepository.GetRefreshTokenModel(refreshToken);
        }

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

            await authRepository.AddUser(newUser);

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

        public async Task<bool> AddRole(RoleModel role)
        {
            return await authRepository.AddRole(role);
        }
    }
}