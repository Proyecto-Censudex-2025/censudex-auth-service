using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class LoginRequest
    {
        /// <summary>
        /// Client's email
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Client's username
        /// </summary>
        public string? Username { get; set; }
        /// <summary>
        /// Client's password
        /// </summary>
        public required string Password { get; set; }
    }
}