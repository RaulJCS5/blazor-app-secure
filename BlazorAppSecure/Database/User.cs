using Microsoft.AspNetCore.Identity;

namespace BlazorAppSecure.Database
{
    public class User : IdentityUser
    {
        public string? Initials { get; set; }
    }
}
