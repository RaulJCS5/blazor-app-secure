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

        public CustomAuthenticationStateProvider(IUserService userService, IRoleService roleService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _roleService = roleService;
            _httpContextAccessor = httpContextAccessor;
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

                    var roles = await _roleService.GetUserRolesAsync(userInfo.Email);

                    if (roles != null && roles.Count > 0)
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
                var result = await _userService.RegisterAsync(email, password);
                if (result != null)
                {
                    return new FormResult { Succeeded = true };
                }
                
                return new FormResult
                {
                    Succeeded = false,
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
                var result = await _userService.LoginAsync(email, password, false);

                if (result != null)
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
            await _userService.LogoutAsync();

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
                var roles = await _roleService.GetRolesAsync();
                if (roles != null)
                {
                    return roles.Select(x => new Role { Name = x.Name }).ToList();
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
                var result = await _roleService.AddRolesAsync(roles);
                if (result.Count == 0)
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
                var userList = await _userService.GetAllUsers();
                if (userList != null)
                {
                    var users = userList.Select(x => new UserViewModel
                    {
                        Email = x.Email,
                        UserName = x.UserName,
                        PhoneNumber = x.PhoneNumber,
                        Roles = x.Roles
                    }).ToArray();
                    return users;
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
                var userModel = await _userService.GetUserById(userEmailId);
                if (userModel != null)
                {
                    var user = new UserViewModel
                    {
                        Email = userModel.Email,
                        UserName = userModel.UserName,
                        PhoneNumber = userModel.PhoneNumber,
                        Roles = userModel.Roles
                    };
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
                // TODO: Do not this UserViewModel to UserModel conversion here.
                var result = await _userService.UpdateUser(userEmailId, new Model.UserModel
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = user.Roles
                });
                return result;
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
                var result = await _userService.DeleteUserByEmail(userEmailId);
                return result;
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}