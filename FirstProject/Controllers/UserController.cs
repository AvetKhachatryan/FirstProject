using FirstProject.Data;
using FirstProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FirstProject.Data.Entities;
using Microsoft.AspNetCore.Authorization;

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
    }
}
