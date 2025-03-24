using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApp.Model;

namespace WebApp
{
    public class CustomAuthStateProvider(IHttpContextAccessor httpContext) : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContext;
        private const string AuthCookieName = "AuthToken"; // Name of the cookie
        private readonly AuthenticationState? _anonymousUser = new(new ClaimsPrincipal(new ClaimsIdentity())); // Anonymous user

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var authCookie = _httpContextAccessor.HttpContext?.Request.Cookies[AuthCookieName];

                if (string.IsNullOrEmpty(authCookie))
                {
                    return _anonymousUser;
                }
                var identity = GetClaimsIdentity(authCookie);
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch (Exception)
            {
                return _anonymousUser;
            }
        }
        // Method to get token from the cookie (used in ApiClient)
        public string GetTokenFromCookie()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies[AuthCookieName];
        }
        public async Task MarkUserAsLoggedOut()
        {
            try
            {
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete(AuthCookieName);

                var identity = new ClaimsIdentity();
                var user = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            }
            catch (Exception)
            {

            }
        }
        public async Task MarkUserAsAuthenticated(LoginResponseModel model)
        {
            try
            {
                var identity = GetClaimsIdentity(model.Token);
                var user = new ClaimsPrincipal(identity);

                // Store token in a secure cookie
                _httpContextAccessor.HttpContext?.Response.Cookies.Append(AuthCookieName, model.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(1) // Set expiration time
                });

                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            }
            catch (Exception)
            {

            }
        }
        private static ClaimsIdentity GetClaimsIdentity(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var claims = jwtToken.Claims;
                return new ClaimsIdentity(claims, "jwt");
            }
            catch (Exception)
            {
                return new ClaimsIdentity();
            }
        }
    }
}
