using Microsoft.EntityFrameworkCore;
using WebApiAuth.Data;
using WebApiAuth.Model;

namespace WebApiAuth.Repository
{
    public class AuthRepository(AppDbContext dbContext) : IAuthRepository
    {
        //  Generic method to check existence of an entity in the database
        private async Task<bool> ExistsAsync<T>(DbSet<T> dbSet, Func<T, bool> predicate) where T : class
        {
            return await Task.Run(() => dbSet.Any(predicate));
        }

        //  Get user with roles by login
        public Task<UserModel?> GetUserByLogin(string username, string password) =>
            dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

        //  Remove refresh token by user ID
        public async Task RemoveRefreshTokenByUserID(int userID)
        {
            var refreshTokens = dbContext.RefreshTokens.Where(rt => rt.UserID == userID);
            if (await refreshTokens.AnyAsync())
            {
                dbContext.RefreshTokens.RemoveRange(refreshTokens);
                await dbContext.SaveChangesAsync();
            }
        }

        //  Add refresh token
        public async Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel)
        {
            await dbContext.RefreshTokens.AddAsync(refreshTokenModel);
            await dbContext.SaveChangesAsync();
        }

        //  Get refresh token with user and roles
        public Task<RefreshTokenModel?> GetRefreshTokenModel(string refreshToken) =>
            dbContext.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.RefreshToken == refreshToken);

        //  Get user by username
        public Task<UserModel?> GetUser(string username) =>
            dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

        //  Add user with duplicate check
        public async Task<bool> AddUser(UserModel user)
        {
            if (await ExistsAsync(dbContext.Users, u => u.Username == user.Username))
                return false;

            await dbContext.Users.AddAsync(user);
            return await dbContext.SaveChangesAsync() > 0;
        }

        //  Get role by role name
        public Task<RoleModel?> GetRole(string roleName) =>
            dbContext.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);

        //  Add user role with duplicate check
        public async Task<bool> AddUserRole(UserRoleModel userRole)
        {
            if (await ExistsAsync(dbContext.UserRoles, ur => ur.UserID == userRole.UserID && ur.RoleID == userRole.RoleID))
                return false;

            await dbContext.UserRoles.AddAsync(userRole);
            return await dbContext.SaveChangesAsync() > 0;
        }

        //  Add a new role with duplicate check
        public async Task<bool> AddRole(RoleModel role)
        {
            if (await ExistsAsync(dbContext.Roles, r => r.RoleName == role.RoleName))
                return false;

            await dbContext.Roles.AddAsync(role);
            return await dbContext.SaveChangesAsync() > 0;
        }

        //  Check if a user has a specific role
        public async Task<bool> UserRoleExists(string username, string roleName)
        {
            var user = await GetUser(username);
            var role = await GetRole(roleName);
            if (user == null || role == null)
                return false;

            return await dbContext.UserRoles.AnyAsync(ur => ur.UserID == user.ID && ur.RoleID == role.ID);
        }
    }
}
