using BlazorAppIdentity.Model;

namespace BlazorAppIdentity.Services
{
    public interface IAccountManagement
    {
        public Task<FormResult> RegisterAsync(string email, string password);

    }
}
