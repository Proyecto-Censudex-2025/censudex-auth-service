using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.src.Interface
{
    public interface ITokenBlocklistService
    {
        Task<bool> AddToBlocklistAsync(string token, DateTime expiresAt);
        Task<bool> IsTokenBlockedAsync(string token);
        void CleanupExpiredTokens();
    }
}