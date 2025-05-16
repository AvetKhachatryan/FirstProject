using FirstProject.Data;
using FirstProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        
    }
}
