using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data.Entities;

namespace FirstProject.Data.Interfaces
{
    public interface IUserRepo
    {
        public Task AddUser(User user);
    }
}
