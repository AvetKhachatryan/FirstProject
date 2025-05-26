using FirstProject.Data;
using FirstProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FirstProject.Models;
using FirstProject.Services.Services;
using Microsoft.EntityFrameworkCore;

namespace FirstProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {

        private IUserService _service;
        private ITokenService _tokenService;
        public AuthenticationController(IUserService service, ApplicationDbContext context, ITokenService tokenService)
        {
            _service = service;
            _tokenService = tokenService;   
        }

        [HttpPost("register")]
        public Task AddUser(RegistrationModel model)
        {
            _service.AddUser(model.Username, model.Password, model.PasswordConfirm, model.Email);
            return Task.CompletedTask;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthenticationModel model)
        {
            var user = _service.GetByUsername(model.Username);
            if (user == null)
                return Unauthorized("User not found");

            var passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            if (!passwordValid)
                return Unauthorized("Invalid password");

            var token = _tokenService.CreateToken(user);

            return Ok(new { token });
        }


    }
}
