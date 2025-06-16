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
        private readonly IUserService _service;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService service, ILogger<UserController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("usersList")]
        public List<User> GetUsers()
        {
            _logger.LogInformation("Admin {User} запросил список пользователей в {Time}", User.Identity.Name, DateTime.UtcNow);
            return _service.GetUsers();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser([FromBody] string username)
        {
            _logger.LogInformation("Admin {User} пытается удалить пользователя {TargetUser} в {Time}", User.Identity.Name, username, DateTime.UtcNow);
            try
            {
                await _service.DeleteUser(username);
                _logger.LogInformation("Пользователь {TargetUser} удалён успешно", username);
                return Ok(new { message = $"Пользователь {username} удалён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя {TargetUser}", username);
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [Authorize]
        [HttpPatch("changeUsername")]
        public IActionResult ChangeUsername([FromBody] string NewUsername)
        {
            if (string.IsNullOrWhiteSpace(NewUsername))
            {
                _logger.LogWarning("Пользователь {User} попытался сменить имя на пустое или пробел", User.Identity.Name);
                return BadRequest("Username is required.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("Неавторизованный пользователь попытался сменить имя");
                return Unauthorized();
            }

            var user = _service.GetbyId(int.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден для смены имени", userId);
                return NotFound("User not found.");
            }

            user.Username = NewUsername;
            _service.UpdateUser(user);
            _logger.LogInformation("Пользователь с ID {UserId} успешно сменил имя на {NewUsername}", userId, NewUsername);

            return Ok(new { message = "Username changed successfully." });
        }

    }
}
