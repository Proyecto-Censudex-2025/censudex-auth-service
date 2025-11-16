using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthService.src.Interface;
using AuthService.src.models;
using AuthService.src.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.src.Controller
{
    /// <summary>
    /// Authentication controller.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // authService connection
        private readonly IAuthService _authService;
        private readonly ITokenBlocklistService _tokenBlocklistService;

        /// <summary>
        /// Constructor that initializes the authService.
        /// </summary>
        /// <param name="ticketRepository">Repositorio de tickets.</param>
        public AuthController(IAuthService authService, ITokenBlocklistService tokenBlocklistService)
        {
            _authService = authService;
            _tokenBlocklistService = tokenBlocklistService;
        }

        /// <summary>
        /// This C# function handles a POST request for client login, validating the request and returning
        /// appropriate responses based on the outcome.
        /// </summary>
        /// <param name="LoginRequest">The `LoginRequest` parameter in the `Login` method represents the
        /// data that is expected to be sent in the body of the HTTP POST request when a client is trying
        /// to log in. This data typically includes the client's credentials such as clientname and
        /// password.</param>
        /// <returns>
        /// The Login method returns an ActionResult of type LoginResponse. Depending on the scenario,
        /// it can return a BadRequest response with the ModelState if the request is not valid, an Ok
        /// response with the login response if successful, an Unauthorized response with a message if
        /// an UnauthorizedAccessException is caught, or a StatusCode 503 response with a message if a
        /// ServiceUnavailableException is caught.
        /// </returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (ServiceUnavailableException ex)
            {
                return StatusCode(503, new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Logout endpoint that adds the current JWT token to the blocklist.
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<ActionResult> Logout()
        {
            try
            {
                // Extract token from Authorization header
                var token = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { Message = "Token not found" });
                }

                // Parse token to get expiration
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var expiresAt = jwtToken.ValidTo;

                // Add token to blocklist
                var result = await _tokenBlocklistService.AddToBlocklistAsync(token, expiresAt);

                if (result)
                {
                    return Ok(new { Message = "Logged out successfully" });
                }

                return StatusCode(500, new { Message = "Failed to logout" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during logout", Error = ex.Message });
            }
        }

        /// <summary>
        /// Validate token endpoint for API Gateway to verify JWT tokens.
        /// Checks both token validity and blocklist status.
        /// </summary>
        [HttpGet("validate")]
        [AllowAnonymous]
        public async Task<ActionResult> ValidateToken()
        {
            // Extract token from Authorization header
            var token = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { 
                    IsValid = false, 
                    Message = "Token is required in Authorization header" 
                });
            }

            try
            {
                // First check if token is in blocklist
                var isBlocked = await _tokenBlocklistService.IsTokenBlockedAsync(token);
                if (isBlocked)
                {
                    return Ok(new { 
                        IsValid = false, 
                        Message = "Token has been revoked" 
                    });
                }

                // Validate token using auth service
                var validationResult = await _authService.ValidateTokenAsync(token);

                if (!validationResult.IsValid)
                {
                    return Ok(new { 
                        IsValid = false, 
                        Message = validationResult.ErrorMessage ?? "Invalid or expired token" 
                    });
                }

                // Return validation success with claims
                return Ok(new { 
                    IsValid = true, 
                    Message = "Token is valid",
                    Claims = validationResult.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    IsValid = false, 
                    Message = "An error occurred during token validation", 
                    Error = ex.Message 
                });
            }
        }
        
    }
}