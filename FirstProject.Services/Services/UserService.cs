using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using FirstProject.Data;
using FirstProject.Data.Entities;
using FirstProject.Data.Interfaces;
using FirstProject.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace FirstProject.Services.Services
{
    public class UserService : IUserService
    {
        private IUserRepo _repo;
        private IConfiguration _config;
        public UserService(IUserRepo repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }


        public User GetByUsername(string username)
        {
            User user = _repo.GetUsers().FirstOrDefault(x => x.Username == username);
            return user;
        }

        public User GetByEmail(string email)
        {
            User user = _repo.GetUsers().FirstOrDefault(x => x.Email == email);
            return user;
        }

        public string GeneratePasswordResetToken(string email)
        {
            var secretKey = _config["ResetPasswordJwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                issuer: _config["ResetPasswordJwt:Issuer"],
                audience: _config["ResetPasswordJwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["ResetPasswordJwt:ExpiresInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public List<User> GetUsers()
        {
            return _repo.GetUsers();
        }


        public Task AddUser(string username, string password, string passwordConfirm, string email, UserRoleType role = UserRoleType.User)
        {
            User user = new User()
            {

                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role, 
                Email = email,
            };

            if (_repo.GetUsers().Any(u => u.Username == username))
            {
                throw new Exception("User with this username already exists");
            }
            _repo.AddUser(user);
            return Task.CompletedTask;
        }

        public Task UpdateUser(User user)
        {
            _repo.UpdateUser(user);
            return Task.CompletedTask;
        }

        public Task DeleteUser(string username)
        {
            _repo.DeleteUser(username);
            return Task.CompletedTask;
        }

        public User GetbyId(int id)
        {
            User user = _repo.GetUsers().FirstOrDefault(x => x.Id == id);
            return user;
        }
    }
}
        //verify

        //public bool VerifyPassword(string plainPassword, string hashedPassword, User user)
        //{
        //    var result = _hasher.VerifyHashedPassword(user, hashedPassword, plainPassword);
        //    return result == PasswordVerificationResult.Success;
        //}