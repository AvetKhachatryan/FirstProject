using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data;
using FirstProject.Data.Entities;
using FirstProject.Data.Interfaces;

namespace FirstProject.Data.Repositories
{
    public class UserRepo : IUserRepo
    {
        private ApplicationDbContext _context;
        public UserRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> GetUsers()
        {
            return _context.Users.ToList();
        }

        public async Task AddUserAsync(User user)
        {
            if (GetUsers().Count == 0)
            {
                user.Role = UserRoleType.Admin;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }


        public Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        public Task DeleteUser(string username)
        {
            User user = _context.Users.FirstOrDefault(x => x.Username == username);
            _context.Users.Remove(user);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

    }
}
