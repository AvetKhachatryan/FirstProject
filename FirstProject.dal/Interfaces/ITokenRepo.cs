using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data.Entities;

namespace FirstProject.Data.Interfaces
{
    public interface ITokenRepo
    {
        Task AddRefreshToken(RefreshToken refreshTokenEntity);
        RefreshToken FindByToken(string token);
        Task UpdateToken(RefreshToken token);
    }
}
