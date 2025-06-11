using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data;
using FirstProject.Data.Entities;

namespace FirstProject.Services.Interfaces
{
    public interface IUserService
    {
        public Task<User> AddUserAsync(string Username, string Password, string passwordConfirm, string Email, UserRoleType Role = UserRoleType.User);
        public Task UpdateUser(User user);
        public Task DeleteUser(string Username);
        public User GetByUsername(string username);
        public User GetByEmail(string email);
        public User GetbyId(int id);
        public string GeneratePasswordResetToken(string email);
        public List<User> GetUsers();
    }
}
