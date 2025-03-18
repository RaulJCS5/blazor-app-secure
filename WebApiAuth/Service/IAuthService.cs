using WebApiAuth.Model;

namespace WebApiAuth.Service
{
    public interface IAuthService
    {
        Task<UserModel> GetUserByLogin(string username, string password);
        Task AddRefreshTokenModel(RefreshTokenModel refreshTokenModel);
        Task<RefreshTokenModel> GetRefreshTokenModel(string refreshToken);
        Task<UserModel?> RegisterUser(RegisterModel registerModel);
        Task<bool> AddRole(RoleModel role);
        Task<bool> AssignRoleToUser(string username, string rolename);
    }
}
