using BlazorAppAuth.Database;
using BlazorAppAuth.Model;
using Microsoft.AspNetCore.Identity;

namespace BlazorAppAuth.Services
{
    public interface IUserService
    {
        Task<User> GetUserInfoAsync(string userId);
        Task<List<UserModel>> GetAllUsers();
        Task<UserModel> GetUserById(string emailId);
        Task<bool> UpdateUser(string emailId, UserModel user);
        Task<bool> DeleteUserByEmail(string emailId);
    }
}
