using WebApiAuth.Model;

namespace WebApiAuth.Repository
{
    public interface IAuthRepository
    {
        Task<UserModel> GetUserByLogin(string username, string password);
        Task RemoveRefreshTokenByUserID(int userID);
        Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel);
        Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken);
        Task<UserModel?> GetUser(string username);
        Task<bool> AddUser(UserModel user);
        Task<RoleModel?> GetRole(string roleName);
        Task<bool> AddUserRole(UserRoleModel userRole);
        Task<bool> AddRole(RoleModel role);
    }
}
