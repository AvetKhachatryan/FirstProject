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
        public Task AddUser(string Username, string Password, string passwordConfirm, string Email, UserRoleType Role = UserRoleType.User);
        public User GetByUsername(string username);
    }
}
