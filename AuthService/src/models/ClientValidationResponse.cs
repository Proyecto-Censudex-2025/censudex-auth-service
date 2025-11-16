using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class ClientValidationResponse
    {
        /// <summary>
        /// Client's state
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// Client's id
        /// </summary>
        public required string Id { get; set; }
        /// <summary>
        /// Client's email
        /// </summary>
        public required string Email { get; set; }
        /// <summary>
        /// Client's Name
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Client's surename
        /// </summary>
        public required string Surename { get; set; }
        /// <summary>
        /// Client's username
        /// </summary>
        public required string Username { get; set; }
        /// <summary>
        /// Client's role
        /// </summary>
        public required string Role { get; set; }
        /// <summary>
        /// Client's claims
        /// </summary>
        public List<Claim> Claims { get; set; } = new();
        /// <summary>
        /// Possible error on the request
        /// </summary>
        public required string ErrorMessage { get; set; }
    }
}