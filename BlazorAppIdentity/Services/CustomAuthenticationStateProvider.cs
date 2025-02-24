using BlazorAppIdentity.Model;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
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

                    var roleResponse = await _client.GetAsync($"api/role/GetUserRoles?emailId={userInfo.Email}");

                    roleResponse.EnsureSuccessStatusCode();

                    var roleJson = await roleResponse.Content.ReadAsStringAsync();

                    var roles = JsonSerializer.Deserialize<string[]>(roleJson, jsonSerializerOptions);

                    if (roles != null && roles.Length > 0)
                    {
                        foreach (var role in roles)
                        {
                            claims.Add(new(ClaimTypes.Role, role));
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
                // if it is not authenticated it should not return an exception
                //throw;
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

        public async Task LogoutAsync()
        {
            const string Empty = "{}";

            var emptyContent = new StringContent(Empty, Encoding.UTF8, "application/json");

            await _client.PostAsync("api/user/Logout", emptyContent);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        }

        public async Task<bool> CheckAuthenticatedAsync()
        {
            await GetAuthenticationStateAsync();
            return _authenticated;
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            try
            {
                var result = await _client.GetAsync("api/role/GetRoles");
                var response = await result.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<List<Role>>(response, jsonSerializerOptions);
                if (result.IsSuccessStatusCode)
                {
                    return roles;
                }
            }
            catch (Exception ex)
            {
                //throw;
            }
            return new List<Role>();
        }

        public async Task<FormResult> AddRoleAsync(string[] roles)
        {
            try
            {

                var content = new StringContent(JsonSerializer.Serialize(roles), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("api/role/AddRoles", content);

                if (result.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }
            }
            catch (Exception ex)
            {
                //throw;
            }

            return new FormResult { Succeeded = false, ErrorList = ["An unknown error prevented the role from being added."] };
        }

        public async Task<UserViewModel[]> GetUsers()
        {
            try
            {
                var result = await _client.GetAsync("api/user");
                var response = await result.Content.ReadAsStringAsync();
                var userlist = JsonSerializer.Deserialize<UserViewModel[]>(response, jsonSerializerOptions);
                if (result.IsSuccessStatusCode)
                {
                    return userlist;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public async Task<UserViewModel> GetUserByEmail(string userEmailId)
        {
            try
            {
                var result = await _client.GetAsync($"api/user/{userEmailId}");
                var response = await result.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserViewModel>(response, jsonSerializerOptions);
                if (result.IsSuccessStatusCode)
                {
                    return user;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public async Task<bool> UserUpdate(string userEmailId, UserViewModel user)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(user),
            Encoding.UTF8, "application/json");
                // Make sure the UserViewModel is fully populated with the correct data
                // For example I got problems because the PhoneNumbe was not set
                // And it always returned 400 Bad Request
                // Message=Value cannot be null.
                var response = await _client.PutAsync($"api/User/{userEmailId}", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {responseContent}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<bool> UserDelete(string userEmailId)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/User/{userEmailId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}