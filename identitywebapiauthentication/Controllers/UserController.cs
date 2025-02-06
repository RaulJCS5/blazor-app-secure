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

        public UserController(SignInManager<IdentityUser> signInManager)
        {
            _signInManaManager = signInManager;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("my user list");
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
