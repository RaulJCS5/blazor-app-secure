using BlazorAppIdentity.Model;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorAppIdentity.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IAccountManagement
    {
        private bool _authenticated = false;

        private readonly ClaimsPrincipal Unauthenticated = new ClaimsPrincipal(new ClaimsIdentity());

        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public CustomAuthenticationStateProvider(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("Auth");
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _authenticated = false;

            var user = Unauthenticated;

            try
            {
                var userResponse = await _client.GetAsync("manage/info");

                userResponse.EnsureSuccessStatusCode();

                var userJson = await userResponse.Content.ReadAsStringAsync();

                var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, jsonSerializerOptions);

                if (userInfo != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userInfo.Email),
                        new Claim(ClaimTypes.Email, userInfo.Email),
                    };
                    claims.AddRange(
                        userInfo.Claims.Where(c => c.Key != ClaimTypes.Name && c.Key != ClaimTypes.Email).Select(c => new Claim(c.Key, c.Value))
                    );

                    var roleResponse = await _client.GetAsync($"api/Role/GetUserRoles?userEmail={userInfo.Email}");

                    roleResponse.EnsureSuccessStatusCode();

                    var roleJson = await roleResponse.Content.ReadAsStringAsync();

                    var roles = JsonSerializer.Deserialize<string[]>(roleJson, jsonSerializerOptions);

                    if (roles != null && roles.Length > 0)
                    {
                        foreach (var role in roles)
                        {
                            claims.Add(new (ClaimTypes.Role, role));
                        }
                    }
                    // Create a new ClaimsIdentity with the claims and the cookie authentication
                    var id = new ClaimsIdentity(claims, nameof(CustomAuthenticationStateProvider));

                    user = new ClaimsPrincipal(id);

                    _authenticated = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return new AuthenticationState(user);
        }

        public async Task<FormResult> RegisterAsync(string email, string password)
        {
            string[] defaultDetail = ["An unkown error prevented registration from succeeding."];

            try
            {
                var result = await _client.PostAsJsonAsync("register", new { email, password });
                if (result.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }
                var error = await result.Content.ReadAsStringAsync();
                var problemDetails = JsonDocument.Parse(error);
                var errors = new List<string>();
                var errorList = problemDetails.RootElement.GetProperty("errors");

                foreach (var item in errorList.EnumerateObject())
                {
                    if (item.Value.ValueKind == JsonValueKind.String)
                    {
                        errors.Add(item.Value.GetString());
                    }
                    else if (item.Value.ValueKind == JsonValueKind.Array)
                    {
                        errors.AddRange(item.Value.EnumerateArray().Select(x => x.GetString() ?? string.Empty).Where(x => !string.IsNullOrWhiteSpace(x)));
                    }
                }
                return new FormResult
                {
                    Succeeded = false,
                    ErrorList = problemDetails == null ? defaultDetail : [.. errors]
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<FormResult> LoginAsync(string email, string password)
        {
            try
            {
                var result = await _client.PostAsJsonAsync("login?useCookies=true", new { email, password });

                if (result.IsSuccessStatusCode)
                {
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                    return new FormResult { Succeeded = true };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return new FormResult { Succeeded = false, ErrorList = ["Invalid login attempt."] };
        }
    }
}
