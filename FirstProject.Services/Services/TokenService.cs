using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data.Entities;
using FirstProject.Data.Interfaces;
using FirstProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace FirstProject.Services.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private ITokenRepo _repo;

        public TokenService(IConfiguration config, ITokenRepo repo)
        {
            _config = config;
            _repo = repo;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiresInMinutes"])),
            signingCredentials: credentials
        );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user)
        {
            // Access Token
            var accessToken = GenerateAccessToken(user);

            // Refresh Token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshToken:ExpiresInDays"])),
                IsUsed = false,
                IsRevoked = false
            };

            _repo.AddRefreshToken(refreshTokenEntity);

            return (accessToken, refreshToken);
        }

        public RefreshToken FindByToken(string token)
        {
            return _repo.FindByToken(token);
        }

        public Task UpdateToken(RefreshToken token)
        {
            _repo.UpdateToken(token);
            return Task.CompletedTask;
        }
    }
}
