using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace WebApp
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState? _cachedAuthState;
        private LoginResponseModel? _sessionState; // Store in-memory session state
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_cachedAuthState != null)
            {
                return _cachedAuthState;
            }

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            _cachedAuthState = new AuthenticationState(anonymousUser);
            return _cachedAuthState;
        }
    }
}
