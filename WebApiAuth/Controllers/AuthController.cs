using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAuth.Dto;
using WebApiAuth.Model;
using WebApiAuth.Service;

namespace WebApiAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IConfiguration configuration, IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginModel loginModel)
        {
            var user = await authService.GetUserByLogin(loginModel.Username, loginModel.Password);
            if (user != null)
            {
                var token = GenerateJwtToken(user, isRefreshToken: false);
                var refreshToken = GenerateJwtToken(user, isRefreshToken: true);

                await authService.AddRefreshTokenModel(new RefreshTokenModel
                {
                    RefreshToken = refreshToken,
                    UserID = user.ID
                });

                return Ok(new LoginResponseModel
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    TokenExpired = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
                });
            }
            return BadRequest(new { message = "Invalid login attempt" });
        }
        [HttpGet("loginByRefeshToken")]
        public async Task<ActionResult<LoginResponseModel>> LoginByRefeshToken(string refreshToken)
        {
            var refreshTokenModel = await authService.GetRefreshTokenModel(refreshToken);
            if (refreshTokenModel == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            var newToken = GenerateJwtToken(refreshTokenModel.User, isRefreshToken: false);
            var newRefreshToken = GenerateJwtToken(refreshTokenModel.User, isRefreshToken: true);

            await authService.AddRefreshTokenModel(new RefreshTokenModel
            {
                RefreshToken = newRefreshToken,
                UserID = refreshTokenModel.UserID
            });

            return new LoginResponseModel
            {
                Token = newToken,
                TokenExpired = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds(),
                RefreshToken = newRefreshToken,
            };
        }

        private string GenerateJwtToken(UserModel user, bool isRefreshToken)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Username),
            };
            claims.AddRange(user.UserRoles.Select(n => new Claim(ClaimTypes.Role, n.Role.RoleName)));

            string secret = configuration.GetValue<string>($"Jwt:{(isRefreshToken ? "RefreshTokenSecret" : "Secret")}");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "joncena",
                audience: "joncena",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(isRefreshToken ? 24 * 60 : 30),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var user = await authService.RegisterUser(registerModel);
            if (user == null)
            {
                return BadRequest("User already exists");
            }
            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("addrole")]
        public async Task<IActionResult> AddRole([FromBody] RoleModel roleModel)
        {
            var result = await authService.AddRole(roleModel);

            if (!result)
            {
                return BadRequest(new { message = "Role already exists" });
            }

            return Ok(new { message = "Role created successfully" });
        }
        [HttpPost("assignrole")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
        {
            if (string.IsNullOrWhiteSpace(assignRoleDto.Username) || string.IsNullOrWhiteSpace(assignRoleDto.RoleName))
            {
                return BadRequest(new { message = "Username and RoleName are required." });
            }

            var result = await authService.AssignRoleToUser(assignRoleDto.Username, assignRoleDto.RoleName);

            if (!result)
            {
                return BadRequest(new { message = "Failed to assign role. User or role may not exist, or role is already assigned." });
            }

            return Ok(new { message = "Role assigned successfully" });
        }
        [HttpPost("assignroles")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto assignRolesDto)
        {
            if (string.IsNullOrWhiteSpace(assignRolesDto.Username) || assignRolesDto.RoleNames == null || !assignRolesDto.RoleNames.Any())
            {
                return BadRequest(new { message = "Username and at least one RoleName are required." });
            }

            List<string> assignedRoles = new();
            List<string> failedRoles = new();

            foreach (var roleName in assignRolesDto.RoleNames)
            {
                bool result = await authService.AssignRoleToUser(assignRolesDto.Username, roleName);

                if (result)
                {
                    assignedRoles.Add(roleName);
                }
                else
                {
                    failedRoles.Add(roleName);
                }
            }

            if (!assignedRoles.Any())
            {
                return BadRequest(new { message = "No roles were assigned. User may not exist, roles may not exist, or roles are already assigned." });
            }

            return Ok(new
            {
                message = "Role assignment completed.",
                assignedRoles,
                failedRoles = failedRoles.Any() ? failedRoles : null
            });
        }

    }
}
