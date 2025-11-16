using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.src.models
{
    public class ClientInfo
    {
        /// <summary>
        /// Client's Id
        /// </summary>
        public required string Id { get; set; }
        /// <summary>
        /// Client's email
        /// </summary>
        public required string Email { get; set; }
        /// <summary>
        /// Client's username
        /// </summary>
        public required string Username { get; set; }
        /// <summary>
        /// Client's role
        /// </summary>
        public required string Role { get; set; }
        /// <summary>
        /// Client's fullname (name + surename)
        /// </summary>
        public required string FullName { get; set; }
        
    }
}