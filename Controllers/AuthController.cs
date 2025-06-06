using ParcelAuthAPI.Models;
using ParcelAuthAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ParcelAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterModel model)
        {
            var token = _auth.Register(model);
            return token == null ? BadRequest("User already exists") : Ok(new { token });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginModel model)
        {
            var token = _auth.Login(model);
            return token == null ? Unauthorized() : Ok(new { token });
        }
    }
}
