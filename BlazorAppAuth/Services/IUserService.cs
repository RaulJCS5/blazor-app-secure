using BlazorAppAuth.Database;
using BlazorAppAuth.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorAppAuth.Services
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllUsersAsync();

        Task<UserModel> GetUserByEmailAsync(string userEmail);

        Task<bool> UpdateUserAsync(string userEmail, UserModel userModel);

        Task<bool> DeleteUserAsync(string userEmail);
    }
}
