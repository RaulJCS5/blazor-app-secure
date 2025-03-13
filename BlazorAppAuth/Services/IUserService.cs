using BlazorAppAuth.Database;
using BlazorAppAuth.Model;
using Microsoft.AspNetCore.Identity;

namespace BlazorAppAuth.Services
{
    public interface IUserService
    {
        Task<User> GetUserInfoAsync(string userId);
        Task LogoutAsync();
        Task<SignInResult> LoginAsync(string email, string password, bool rememberMe);
        Task<User> RegisterAsync(string email, string password);
        Task<List<UserModel>> GetAllUsers();
        Task<UserModel> GetUserById(string emailId);
        Task<bool> UpdateUser(string emailId, UserModel user);
        Task<bool> DeleteUserByEmail(string emailId);
    }
}
