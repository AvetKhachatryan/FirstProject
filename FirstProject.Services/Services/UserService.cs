using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using FirstProject.Data;
using FirstProject.Data.Entities;
using FirstProject.Data.Interfaces;
using FirstProject.Services.Interfaces;


namespace FirstProject.Services.Services
{
    public class UserService : IUserService
    {
        private IUserRepo _repo;
        public UserService(ApplicationDbContext context, IUserRepo repo)
        {
            _repo = repo;
        }
        //private PasswordHasher<User> _hasher = new();
        //public string HashPassword(string password, User user)
        //{
        //    return _hasher.HashPassword(user, password);
        //}


        public User GetByUsername(string username)
        {
            User user = _repo.GetUsers().FirstOrDefault(x => x.Username == username);
            return user;
        }

        //verify

        //public bool VerifyPassword(string plainPassword, string hashedPassword, User user)
        //{
        //    var result = _hasher.VerifyHashedPassword(user, hashedPassword, plainPassword);
        //    return result == PasswordVerificationResult.Success;
        //}

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
    }
}