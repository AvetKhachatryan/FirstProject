using FirstProject.Data;
using FirstProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FirstProject.Models;

namespace FirstProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private IUserService _service;
        public AuthenticationController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public Task AddUser(RegistrationModel model)
        {
            _service.AddUser(model.Username, model.Password, model.Email);
            return Task.CompletedTask;
        }
    }
}
