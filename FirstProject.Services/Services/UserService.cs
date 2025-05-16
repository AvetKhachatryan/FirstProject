using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data;
using FirstProject.Data.Entities;
using FirstProject.Data.Interfaces;
using FirstProject.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FirstProject.Services.Services
{
    public class UserService : IUserService
    {
        private IUserRepo _repo;
        public UserService(ApplicationDbContext context, IUserRepo repo)
        {
            _repo = repo;
        }
        private PasswordHasher<User> _hasher = new();
        public string HashPassword(string password, User user)
        {
            return _hasher.HashPassword(user, password);
        }

        //verify

        //public bool VerifyPassword(string plainPassword, string hashedPassword, User user)
        //{
        //    var result = _hasher.VerifyHashedPassword(user, hashedPassword, plainPassword);
        //    return result == PasswordVerificationResult.Success;
        //}
        public Task AddUser(string username, string password, string email, UserRoleType role = UserRoleType.User)
        {
            User user = new User()
            {

                Username = username,
                Password = password,
                Role = role,
                Email = email,
            };
            user.Password = HashPassword(password, user);
            _repo.AddUser(user);
            return Task.CompletedTask;
        }
    }
}