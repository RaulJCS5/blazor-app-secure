using BlazorAppAuth.Services;
using BlazorAppAuth.ViewModel;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BlazorAppAuth.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IAccountManagement
    {
        private bool _authenticated = false;

        private readonly ClaimsPrincipal Unauthenticated = new ClaimsPrincipal(new ClaimsIdentity());

        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;

        public CustomAuthenticationStateProvider(IUserService userService, IRoleService roleService, IHttpContextAccessor httpContextAccessor, ILogger<CustomAuthenticationStateProvider> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _authenticated = false;

            var user = Unauthenticated;

            try
            {
                var userName = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                {
                    return new AuthenticationState(user);
                }

                var userServiceInfo = await _userService.GetUserInfoAsync(userName);

                if (userServiceInfo == null)
                {
                    _logger.LogWarning("User info not found for {UserName}", userName);
                    return new AuthenticationState(user);
                }
                var userJson = JsonSerializer.Serialize(userServiceInfo, jsonSerializerOptions);

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

                    var roles = await _roleService.GetUserRolesAsync(userInfo.Email) ?? new List<string>();

                    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                    user = new ClaimsPrincipal(new ClaimsIdentity(claims, nameof(CustomAuthenticationStateProvider)));

                    _authenticated = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving authentication state.");
            }

            return new AuthenticationState(user);
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
                var roles = await _roleService.GetRolesAsync();
                return roles?.Select(x => new Role { Name = x.Name }).ToList() ?? new List<Role>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles.");
                return new List<Role>();
            }
        }

        public async Task<FormResult> AddRoleAsync(string[] roles)
        {
            try
            {
                var result = await _roleService.AddRolesAsync(roles);
                return result.Count > 0
                    ? new FormResult { Succeeded = true }
                    : new FormResult { Succeeded = false, ErrorList = ["No roles were added."] };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding roles.");
                return new FormResult { Succeeded = false, ErrorList = ["An error occurred while adding roles."] };
            }
        }

        public async Task<UserViewModel[]> GetUsers()
        {
            try
            {
                var userList = await _userService.GetAllUsers();
                return userList?.Select(x => new UserViewModel
                {
                    Email = x.Email,
                    UserName = x.UserName,
                    PhoneNumber = x.PhoneNumber,
                    Roles = x.Roles
                }).ToArray() ?? Array.Empty<UserViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users.");
                return Array.Empty<UserViewModel>();
            }
        }

        public async Task<UserViewModel> GetUserByEmail(string userEmailId)
        {
            try
            {
                var userModel = await _userService.GetUserById(userEmailId);
                return userModel == null ? null : new UserViewModel
                {
                    Email = userModel.Email,
                    UserName = userModel.UserName,
                    PhoneNumber = userModel.PhoneNumber,
                    Roles = userModel.Roles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {UserEmail}", userEmailId);
                return null;
            }
        }

        public async Task<bool> UserUpdate(string userEmailId, UserViewModel user)
        {
            try
            {
                return await _userService.UpdateUser(userEmailId, new Model.UserModel
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = user.Roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserEmail}", userEmailId);
                return false;
            }
        }

        public async Task<bool> UserDelete(string userEmailId)
        {
            try
            {
                return await _userService.DeleteUserByEmail(userEmailId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserEmail}", userEmailId);
                return false;
            }
        }
    }
}