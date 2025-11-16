using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class TokenValidationRequest
    {
        /// <summary>
        /// Token with the client's claims
        /// </summary>
        public required string Token { get; set; }
    }
}