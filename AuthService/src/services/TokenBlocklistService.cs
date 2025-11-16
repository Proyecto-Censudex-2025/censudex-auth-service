using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.src.Interface;
using AuthService.src.models;

namespace authservice.src.Service
{
    public class TokenBlocklistService: ITokenBlocklistService
    {
        private readonly ConcurrentDictionary<string, BlockedToken> _blockedTokens;

        public TokenBlocklistService()
        {
            _blockedTokens = new ConcurrentDictionary<string, BlockedToken>();
        }

        public Task<bool> AddToBlocklistAsync(string token, DateTime expiresAt)
        {
            var blockedToken = new BlockedToken
            {
                Token = token,
                BlockedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };

            var result = _blockedTokens.TryAdd(token, blockedToken);
            return Task.FromResult(result);
        }

        public Task<bool> IsTokenBlockedAsync(string token)
        {
            if (_blockedTokens.TryGetValue(token, out var blockedToken))
            {
                // Check if token has expired
                if (blockedToken.ExpiresAt > DateTime.UtcNow)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    // Remove expired token
                    _blockedTokens.TryRemove(token, out _);
                }
            }

            return Task.FromResult(false);
        }

        public void CleanupExpiredTokens()
        {
            var expiredTokens = _blockedTokens
                .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var token in expiredTokens)
            {
                _blockedTokens.TryRemove(token, out _);
            }
        }

        public int GetBlockedTokenCount()
        {
            return _blockedTokens.Count;
        }
    }
}