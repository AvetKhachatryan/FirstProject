﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstProject.Data.Entities;

namespace FirstProject.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user);
        RefreshToken FindByToken(string token);
        Task UpdateToken(RefreshToken token);
    }
}
