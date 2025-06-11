using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FirstProject.Data;
using FirstProject.Data.Entities;
using FirstProject.Models;
using FirstProject.Services.Interfaces;
using FirstProject.Services.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private readonly ILogger<UserController> _logger;
        public AuthenticationController(IUserService service, IEmailService emailService,
                                ITokenService tokenService, IConfiguration config, ILogger<UserController> logger)
        {
            _service = service;
            _tokenService = tokenService;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> AddUser(RegistrationModel model)
        {
            var user = await _service.AddUserAsync(model.Username, model.Password, model.PasswordConfirm, model.Email);
            if (user == null)
                return BadRequest("Не удалось создать пользователя");

            _logger.LogInformation("Создан новый пользователь с ID {UserId}", user.Id);
            return Ok(user);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticationModel model)
        {
            var user = _service.GetByUsername(model.Username);
            if (user == null)
            {
                _logger.LogWarning("Попытка входа с несуществующим пользователем: {Username}", model.Username);
                return Unauthorized("User not found");
            }
            var passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Неверный пароль для пользователя: {Username}", model.Username);
                return Unauthorized("Invalid password");
            }
            var tokens = await _tokenService.GenerateTokensAsync(user);

            // Сохраняем refresh токен в HttpOnly cookie
            Response.Cookies.Append("refreshToken", tokens.refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // только если HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshToken:ExpiresInDays"]))
            });

            _logger.LogInformation("Пользователь {Username} успешно вошёл в систему", model.Username);

            return Ok(new { tokens.refreshToken, tokens.accessToken });
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string RefreshToken)
        {
            _logger.LogInformation("Запрос на обновление токена получен.");

            var refreshToken = RefreshToken;

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Пустой refresh token в запросе.");
                return Unauthorized("No refresh token found");
            }

            var tokenInDb = _tokenService.FindByToken(refreshToken);

            if (tokenInDb == null || tokenInDb.IsRevoked || tokenInDb.IsUsed || tokenInDb.Expires < DateTime.UtcNow)
            {
                _logger.LogWarning("Невалидный или просроченный refresh token: {Token}", refreshToken);
                return Unauthorized("Invalid refresh token");
            }

            tokenInDb.IsUsed = true;
            await _tokenService.UpdateToken(tokenInDb);
            _logger.LogInformation("Refresh token {Token} помечен как использованный.", refreshToken);

            var user = _service.GetbyId(tokenInDb.UserId);
            if (user == null)
            {
                _logger.LogError("Пользователь с ID {UserId} не найден при обновлении токена.", tokenInDb.UserId);
                return Unauthorized("User not found");
            }

            var (newAccessToken, newRefreshToken) = await _tokenService.GenerateTokensAsync(user);

            _logger.LogInformation("Успешно сгенерированы новые токены для пользователя ID {UserId}.", user.Id);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }



        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            _logger.LogInformation("Запрос на сброс пароля для email: {Email}", request.Email);

            if (string.IsNullOrEmpty(request.Email))
            {
                _logger.LogWarning("ForgotPassword вызван без Email");
                return BadRequest("Email is required.");
            }

            var user = _service.GetByEmail(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с email {Email} не найден при попытке сброса пароля", request.Email);
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

            _logger.LogInformation("Создан токен для сброса пароля пользователя {Email}", request.Email);

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

            _logger.LogInformation("Отправлено письмо со ссылкой на сброс пароля пользователю {Email}", user.Email);

            return Ok(new { message = "If the email exists, password reset instructions have been sent." });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _logger.LogInformation("Попытка сброса пароля с токеном {Token}", request.Token);

            if (request.NewPassword != request.ConfirmPassword)
            {
                _logger.LogWarning("Пароли не совпадают при сбросе пароля");
                return BadRequest("Passwords don't match");
            }

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
            {
                _logger.LogWarning("Невозможно прочитать токен при сбросе пароля");
                return BadRequest("Cannot read token");
            }

            var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken || jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
            {
                _logger.LogWarning("Неверный алгоритм токена при сбросе пароля");
                return Unauthorized("Invalid token algorithm");
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Пустой email в токене сброса пароля");
                return BadRequest("Invalid token payload");
            }

            var user = _service.GetByEmail(email);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с email {Email} не найден при сбросе пароля", email);
                return NotFound("User not found");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _service.UpdateUser(user);

            _logger.LogInformation("Пароль успешно сброшен для пользователя {Email}", email);

            return Content("<h2>Password Reseted!</h2>", "text/html");
        }
    }
}