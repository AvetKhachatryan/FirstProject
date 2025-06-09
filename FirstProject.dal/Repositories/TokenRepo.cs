using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data.Entities;
using FirstProject.Data.Interfaces;
using Microsoft.Identity.Client;

namespace FirstProject.Data.Repositories
{
    public class TokenRepo : ITokenRepo
    {
        private ApplicationDbContext _context;
        public TokenRepo(ApplicationDbContext context)
        { 
            _context = context;
        }

        public async Task AddRefreshToken(RefreshToken refreshTokenEntity)
        {
            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();
        }

        public RefreshToken FindByToken(string token)
        {
            return _context.RefreshTokens.FirstOrDefault(x => x.Token == token);
        }

        public Task UpdateToken(RefreshToken token)
        {
            _context.Update(token);
            _context.SaveChanges();
            return Task.CompletedTask;
        }
    }
}
