using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class LoginResponse
    {
        /// <summary>
        /// Token with the client's claims
        /// </summary>
        public required string Token { get; set; }
        /// <summary>
        ///  Date in wich the token expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// Client's information
        /// </summary>
        public required ClientInfo Client { get; set; }
    }
}