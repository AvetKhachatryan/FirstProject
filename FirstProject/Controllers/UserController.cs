using FirstProject.Data;
using FirstProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FirstProject.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FirstProject.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private IUserService _service;
        public UserController(IUserService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("usersList")]
        public List<User> GetUsers()
        {
            return _service.GetUsers();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("deleteUser")]
        public Task DeleteUser([FromBody] string username)
        {
            _service.DeleteUser(username);
            return Task.CompletedTask;
        }

        [Authorize]
        [HttpPatch("changeUsername")]
        public IActionResult ChangeUsername([FromBody] string NewUsername)
        {
            if (string.IsNullOrWhiteSpace(NewUsername))
                return BadRequest("Username is required.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = _service.GetbyId(int.Parse(userId));
            if (user == null)
                return NotFound("User not found.");

            user.Username = NewUsername;
            _service.UpdateUser(user);

            return Ok(new { message = "Username changed successfully." });
        }
    }
}
