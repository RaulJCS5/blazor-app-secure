using identitywebapiauthentication.Services;
using identitywebapiauthentication.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Identity;

namespace identitywebapiauthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetRolesAsync();
            return Ok(roles);
        }

        [Authorize(Roles = "admin, user")]
        [HttpGet]
        [Route("GetUserRoles")]
        public async Task<IActionResult> GetUserRoles(string emailId)
        {
            var roles = await _roleService.GetUserRolesAsync(emailId);
            return Ok(roles);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("AddRoles")]
        public async Task<IActionResult> AddRoles(string[] roles)
        {
            var result = await _roleService.AddRolesAsync(roles);
            if (result.Count == 0)
            {
                return BadRequest("Roles already exists");
            }
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("AddUserRole")]
        public async Task<IActionResult> AddUserRole([FromBody] AddUserModel addUser)
        {
            var result = await _roleService.AddUserRoleAsync(addUser.EmailId, addUser.Roles);
            if(!result)
            {
                return BadRequest("User not found or roles not exists");
            }
            return StatusCode((int)HttpStatusCode.Created, result);
        }
        // New endpoint to create a role without requiring an existing admin role
        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("CreateRole")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name cannot be empty");
            }

            var roleExists = await _roleService.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return BadRequest("Role already exists");
            }

            var result = await _roleService.CreateRoleAsync(roleName);
            if (result)
            {
                return Ok($"Role '{roleName}' created successfully");
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "Error creating role");
        }
        // New endpoint to assign a user to a role
        [Authorize(Roles = "admin")]
        [HttpPost]
        [Route("AssignUserRole")]
        public async Task<IActionResult> AssignUserRole(string email, string roleName)
        {
            var result = await _roleService.AssignUserRoleAsync(email, roleName);
            if (result)
            {
                return Ok($"User '{email}' assigned to role '{roleName}' successfully");
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "Error assigning user to role");
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        [Route("UpdateRole")]
        public async Task<IActionResult> UpdateRole(string roleName, string newRoleName)
        {
            var result = await _roleService.UpdateRoleAsync(roleName, newRoleName);
            if (result)
            {
                return Ok($"Role '{roleName}' updated to '{newRoleName}' successfully");
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "Error updating role");
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        [Route("UpdateUserRole")]
        public async Task<IActionResult> UpdateUserRole(string email, string currentRoleName, string newRoleName)
        {
            var result = await _roleService.UpdateUserRoleAsync(email, currentRoleName, newRoleName);
            if (result)
            {
                return Ok($"User '{email}' role updated from '{currentRoleName}' to '{newRoleName}' successfully");
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "Error updating user role");
        }

        [Authorize(Roles = "admin")]
        [HttpDelete]
        [Route("RemoveRole")]
        public async Task<IActionResult> RemoveRole(string roleName)
        {
            var result = await _roleService.RemoveRoleAsync(roleName);
            if (result)
            {
                return Ok($"Role '{roleName}' removed successfully");
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "Error removing role");
        }

        [Authorize(Roles = "admin")]
        [HttpDelete]
        [Route("RemoveUserRole")]
        public async Task<IActionResult> RemoveUserRole(string email, string roleName)
        {
            var result = await _roleService.RemoveUserRoleAsync(email, roleName);
            if (result)
            {
                return Ok($"User '{email}' removed from role '{roleName}' successfully");
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "Error removing user from role");
        }
    }
}
