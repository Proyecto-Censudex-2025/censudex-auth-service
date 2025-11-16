using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthService.src.Interface;
using AuthService.src.models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.src.Service
{
    public class TokenService : ITokenService
    {
        /* This part of the code defines a class named `TokenService` that implements the
        `ITokenService` interface. Within this class, the constructor `TokenService` takes an
        `IConfiguration` object as a parameter. The constructor initializes several private readonly
        fields: */
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _jwtExpirationMinutes;
        
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSecret = _configuration["Jwt:Key"];
            _issuer = _configuration["Jwt:Issuer"];
            _audience = _configuration["Jwt:Audience"];
            _jwtExpirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        }
        
        /// <summary>
        /// The function `GenerateJwtToken` creates a JWT token with specified claims, issuer, audience,
        /// expiration time, and signing credentials.
        /// </summary>
        /// <param name="claims">The `GenerateJwtToken` method takes a list of `Claim` objects as input.
        /// These claims represent the information that will be included in the JWT token. Each `Claim`
        /// object typically consists of a claim type and a claim value.</param>
        /// <returns>
        /// The method `GenerateJwtToken` returns a JWT (JSON Web Token) string generated based on the
        /// provided list of claims and configured settings such as issuer, audience, expiration time,
        /// and signing credentials.
        /// </returns>
        public string GenerateJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // Add standard claims
            var allClaims = new List<Claim>(claims)
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };
            
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: allClaims,
                expires: DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                signingCredentials: credentials
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        /// <summary>
        /// The function `ValidateToken` validates a JWT token based on specified parameters and returns
        /// a `ClaimsPrincipal` if the token is valid.
        /// </summary>
        /// <param name="token">The `token` parameter in the `ValidateToken` method is the JWT (JSON Web
        /// Token) that needs to be validated. This token is passed to the method for validation against
        /// the specified validation parameters and secret key.</param>
        /// <returns>
        /// The method `ValidateToken` returns a `ClaimsPrincipal` object if the token is successfully
        /// validated and passes all the validation checks. If the token is invalid, expired, has an
        /// invalid signature, or fails validation for any reason, the method returns `null`.
        /// </returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;
            
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSecret);
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
                };
                
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                // Additional validation - ensure it's a JWT token
                if (validatedToken is not JwtSecurityToken jwtToken || 
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                // Token is expired
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                // Token signature is invalid
                return null;
            }
            catch (SecurityTokenValidationException)
            {
                // Token validation failed
                return null;
            }
            catch (Exception)
            {
                // Any other exception
                return null;
            }
        }
        
        /// <summary>
        /// The IsTokenValid function checks if a token is valid by validating it and returning a
        /// boolean result.
        /// </summary>
        /// <param name="token">A string representing a token that needs to be validated.</param>
        /// <returns>
        /// The method `IsTokenValid` is returning a boolean value, which indicates whether the token is
        /// valid or not. It returns `true` if the `principal` object returned by the `ValidateToken`
        /// method is not null, and `false` otherwise.
        /// </returns>
        public bool IsTokenValid(string token)
        {
            var principal = ValidateToken(token);
            return principal != null;
        }
    }
}