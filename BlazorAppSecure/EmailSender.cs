using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace BlazorAppSecure
{

    public class EmailSender<TUser> : IEmailSender<TUser> where TUser : class
    {
        public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
        {
            // Implement your logic here
            return Task.CompletedTask;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Implement your logic here
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
        {
            // Implement your logic here
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
        {
            // Implement your logic here
            return Task.CompletedTask;
        }
    }
}
