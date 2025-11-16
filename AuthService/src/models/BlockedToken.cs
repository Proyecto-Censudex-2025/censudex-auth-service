using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class BlockedToken
    {
        /// <summary>
        /// Token with the client's claims
        /// </summary>
        public required string Token { get; set; }
        /// <summary>
        /// Date in wich the Token is added to blacklist
        /// </summary>
        public DateTime BlockedAt { get; set; }
        /// <summary>
        /// Date in wich the Token the token is supposed to expire
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}