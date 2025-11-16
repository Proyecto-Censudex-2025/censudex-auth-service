using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class BlockedToken
    {
        public required string Token { get; set; }
        public DateTime BlockedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}