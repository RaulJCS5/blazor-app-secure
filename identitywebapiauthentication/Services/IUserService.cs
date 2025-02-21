using identitywebapiauthentication.Model;

namespace identitywebapiauthentication.Services
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllUsers();
        Task<UserModel> GetUserById(string emailId);
        Task<bool> UpdateUser(string emailId, UserModel user);
        Task<bool> DeleteUserByEmail(string emailId);
    }
}
