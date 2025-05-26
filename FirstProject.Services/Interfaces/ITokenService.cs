using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data.Entities;

namespace FirstProject.Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
