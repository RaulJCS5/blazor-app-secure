using Microsoft.EntityFrameworkCore;
using WebApiAuth.Data;
using WebApiAuth.Model;

namespace WebApiAuth.Repository
{
    public class AuthRepository(AppDbContext dbContext) : IAuthRepository
    {
        public Task<UserModel> GetUserByLogin(string username, string password)
        {
            return dbContext.Users.Include(n => n.UserRoles).ThenInclude(n => n.Role).FirstOrDefaultAsync(n => n.Username == username && n.Password == password);
        }
        public async Task RemoveRefreshTokenByUserID(int userID)
        {
            var refreshToken = dbContext.RefreshTokens.FirstOrDefault(n => n.UserID == userID);
            if (refreshToken != null)
            {
                dbContext.RemoveRange(refreshToken);
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            await dbContext.RefreshTokens.AddAsync(refreshTokenModel);
            await dbContext.SaveChangesAsync();
        }

        public Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken)
        {
            return dbContext.RefreshTokens.Include(n => n.User).ThenInclude(n => n.UserRoles).ThenInclude(n => n.Role).FirstOrDefaultAsync(n => n.RefreshToken == refreshToken);
        }

        public async Task<UserModel?> GetUser(string username)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(n => n.Username == username);
            return user;
        }

        public async Task<bool> AddUser(UserModel user)
        {
            // Check if user already exists
            var existingUser = await dbContext.Users.AnyAsync(u => u.Username == user.Username);
            if (existingUser)
            {
                return false; // User already exists
            }

            // Add user
            await dbContext.Users.AddAsync(user);
            var result = await dbContext.SaveChangesAsync();

            // Return true if at least one record was added
            return result > 0;
        }


        public async Task<RoleModel?> GetRole(string roleName)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            return role;
        }

        public async Task<bool> AddUserRole(UserRoleModel userRole)
        {
            // Check if the user already has this role
            bool roleExists = await dbContext.UserRoles.AnyAsync(ur => ur.UserID == userRole.UserID && ur.RoleID == userRole.RoleID);

            if (roleExists)
            {
                return false; // Role already assigned to the user
            }

            // Add the role
            await dbContext.UserRoles.AddAsync(userRole);
            int result = await dbContext.SaveChangesAsync();

            // Return true if role was added, null if failed
            return result > 0 ? true : false;
        }

        public async Task<bool> AddRole(RoleModel role)
        {
            // Check if the role already exists
            bool roleExists = await dbContext.Roles.AnyAsync(r => r.RoleName == role.RoleName);

            if (roleExists)
            {
                return false; // Role already exists
            }

            // Add the role
            await dbContext.Roles.AddAsync(role);
            int result = await dbContext.SaveChangesAsync();

            return result > 0;
        }
    }
}