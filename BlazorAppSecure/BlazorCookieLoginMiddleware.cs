using BlazorAppSecure.Database;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace BlazorAppSecure
{
    public class LoginInfo
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public class BlazorCookieLoginMiddleware
    {
        public static IDictionary<Guid, LoginInfo> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, LoginInfo>();


        private readonly RequestDelegate _next;

        public BlazorCookieLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, SignInManager<User> signInMgr)
        {
            if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);
                var info = Logins[key];

                var result = await signInMgr.PasswordSignInAsync(info.Username, info.Password, false, lockoutOnFailure: true);
                info.Password = null;
                if (result.Succeeded)
                {
                    Logins.Remove(key);
                    context.Response.Redirect("/");
                    return;
                }
                else if (result.RequiresTwoFactor)
                {
                    //TODO: redirect to 2FA razor component
                    context.Response.Redirect("/loginwith2fa/" + key);
                    return;
                }
                else
                {
                    //TODO: Proper error handling
                    context.Response.Redirect("/loginfailed");
                    return;
                }
            }
            else if (context.Request.Path == "/Account/Logout")
            {
                await signInMgr.SignOutAsync();
                context.Response.Redirect("/");
                return;
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}
