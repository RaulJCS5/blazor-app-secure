using Microsoft.AspNetCore.Identity;

namespace BlazorAppAuth.Database
{
    public class User : IdentityUser
    {
        public string? Initials { get; set; }
    }
}
