using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data;

namespace FirstProject.Services.Interfaces
{
    public interface IUserService
    {
        public Task AddUser(string Username, string Password, string Email, UserRoleType Role = UserRoleType.User);
    }
}
