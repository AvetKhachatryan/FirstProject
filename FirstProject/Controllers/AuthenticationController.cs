using FirstProject.Data;
using FirstProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FirstProject.Models;
using FirstProject.Services.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ForgotPasswordRequest = FirstProject.Models.ForgotPasswordRequest;
using ResetPasswordRequest = FirstProject.Models.ResetPasswordRequest;

namespace FirstProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {

        private IUserService _service;
        private ITokenService _tokenService;
        private IEmailService _emailService;
        private IConfiguration _config;
        public AuthenticationController(IUserService service, IEmailService emailService,
                                ITokenService tokenService, IConfiguration config)
        {
            _service = service;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
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



        [HttpPost("forgotPassword")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required.");

            var user = _service.GetByEmail(request.Email);
            if (user == null)
            {
                return Ok(new { message = "If the email exists, password reset instructions have been sent." });
            }

            var resetToken = _service.GeneratePasswordResetToken(request.Email);

            var resetLink = Url.Action(
                 "ResetPassword",
                 "Authentication",
                 new { resetToken },
                 protocol: Request.Scheme,
                 host: Request.Host.ToString()
             );

            string body = $@"
                            <html>
                            <body>
                                <p>Для сброса пароля нажмите на ссылку ниже:</p>
                                <p><a href='{resetLink}'>Сбросить пароль</a></p>
                            </body>
                            </html>
                        ";

            _emailService.SendResetLink(
                to: user.Email,
                subject: "Password Reset",
                body: body
            );

            return Ok(new { message = "If the email exists, password reset instructions have been sent." });
        }


        [HttpPost("resetPassword")]
        public IActionResult ResetPassword([FromBody]  ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Passwords don't match");

            // Валидация токена
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["ResetPasswordJwt:Key"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config["ResetPasswordJwt:Issuer"],
                ValidAudience = _config["ResetPasswordJwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (!tokenHandler.CanReadToken(request.Token))
                return BadRequest("Cannot read token");

            var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
                return Unauthorized("Invalid token algorithm");

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return BadRequest("Invalid token payload");

            var user = _service.GetByEmail(email);
            if (user == null)
                return NotFound("User not found");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _service.UpdateUser(user);

            return Content("<h2>Password Reseted!</h2>", "text/html");
        }
    }



}
