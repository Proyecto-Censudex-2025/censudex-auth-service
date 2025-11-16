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
        /* The code snippet is defining a class named `TokenBlocklistService` in C#. */
        private readonly ConcurrentDictionary<string, BlockedToken> _blockedTokens;

        public TokenBlocklistService()
        {
            _blockedTokens = new ConcurrentDictionary<string, BlockedToken>();
        }

        /// <summary>
        /// The function `AddToBlocklistAsync` adds a token to a blocklist with an expiration date and
        /// returns a task indicating success or failure.
        /// </summary>
        /// <param name="token">The `token` parameter is a unique identifier or key that you want to add
        /// to a blocklist. It could be a user session token, an access token, or any other token that
        /// needs to be blocked from further use.</param>
        /// <param name="DateTime">The `DateTime` data type in C# represents a date and time value. It
        /// can store both the date and time components. You can use it to represent specific points in
        /// time or time durations.</param>
        /// <returns>
        /// The method `AddToBlocklistAsync` is returning a `Task<bool>`. The task is being used to
        /// asynchronously add a token to a blocklist and the boolean value indicates whether the token
        /// was successfully added to the blocklist or not.
        /// </returns>
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

        /// <summary>
        /// The function `IsTokenBlockedAsync` checks if a token is blocked and not expired in a
        /// dictionary of blocked tokens.
        /// </summary>
        /// <param name="token">A string representing a token that needs to be checked for
        /// blocking.</param>
        /// <returns>
        /// A `Task<bool>` is being returned. The method `IsTokenBlockedAsync` checks if a token is
        /// blocked and not expired in a dictionary of blocked tokens. If the token is found and not
        /// expired, it returns `Task.FromResult(true)`, indicating that the token is blocked. If the
        /// token is not found or has expired, it returns `Task.FromResult(false)`, indicating that the
        /// token is
        /// </returns>
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

        /// <summary>
        /// The `CleanupExpiredTokens` method removes expired tokens from a collection based on their
        /// expiration time.
        /// </summary>
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

        /// <summary>
        /// This C# function returns the count of blocked tokens stored in the _blockedTokens
        /// collection.
        /// </summary>
        /// <returns>
        /// The method `GetBlockedTokenCount` is returning the count of items in the `_blockedTokens`
        /// collection.
        /// </returns>
        public int GetBlockedTokenCount()
        {
            return _blockedTokens.Count;
        }
    }
}