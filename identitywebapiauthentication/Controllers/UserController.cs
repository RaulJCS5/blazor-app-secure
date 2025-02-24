using identitywebapiauthentication.Model;
using identitywebapiauthentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace identitywebapiauthentication.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManaManager;
        private readonly IUserService _userService;

        public UserController(SignInManager<IdentityUser> signInManager, IUserService userService)
        {
            _signInManaManager = signInManager;
            _userService = userService;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userList = await _userService.GetAllUsers();
            return Ok(userList);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{emailId}")]
        public async Task<IActionResult> Get(string emailId)
        {
            var user = await _userService.GetUserById(emailId);
            return Ok(user);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{emailId}")]
        public async Task<IActionResult> UpdateUser(string emailId, [FromBody] UserModel user)
        {
            var result = await _userService.UpdateUser(emailId, user);
            if (!result)
            {
                return BadRequest();
            }
            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{emailId}")]
        public async Task<IActionResult> DeleteUser(string emailId)
        {
            var result = await _userService.DeleteUserByEmail(emailId);
            if (!result)
            {
                return BadRequest();
            }
            return NoContent();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] object empty)
        {
            if (empty is not null)
            {
                await _signInManaManager.SignOutAsync();
            }
            return Ok();
        }

    }
}
