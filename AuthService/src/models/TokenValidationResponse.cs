using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class TokenValidationResponse
    {
        /// <summary>
        /// state of the token
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// Client's claims
        /// </summary>
        public List<Claim> Claims { get; set; } = new();
        /// <summary>
        /// Possible errors in the request
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}