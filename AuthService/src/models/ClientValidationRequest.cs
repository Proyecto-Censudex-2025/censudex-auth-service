using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class ClientValidationRequest
    {
        /// <summary>
        /// Client's email
        /// </summary>
        public required string? Email { get; set; }
        /// <summary>
        /// Client's username
        /// </summary>
        public required string? Username { get; set; }
        /// <summary>
        /// Client's password
        /// </summary>
        public required string Password { get; set; }
    }
}