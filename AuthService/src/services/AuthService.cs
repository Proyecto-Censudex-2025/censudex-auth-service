using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AuthService.src.Interface;
using AuthService.src.models;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.src.Service
{
    public class AuthService : IAuthService
    {
        /* This code snippet is defining a constructor for the `AuthService` class in C#. The
        constructor takes four parameters: `HttpClient httpClient`, `ITokenService tokenService`,
        `IConfiguration configuration`, and `IMemoryCache cache`. These parameters are then assigned
        to the corresponding private readonly fields within the class. */
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly string _clientApiBaseUrl;

        public AuthService(HttpClient httpClient, ITokenService tokenService, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _configuration = configuration;
            _cache = cache;
            _clientApiBaseUrl = _configuration["ClientAPI:BaseUrl"];
        }

        /// <summary>
        /// The function `GetServiceTokenAsync` retrieves a service token from cache if available,
        /// otherwise returns "no token".
        /// </summary>
        /// <returns>
        /// If the token is found in the cache, the cached token will be returned. Otherwise, "no token"
        /// will be returned.
        /// </returns>
        public async Task<string> GetServiceTokenAsync()
        {
            const string cacheKey = "service_token";

            if (_cache.TryGetValue(cacheKey, out string cachedToken))
            {
                return cachedToken;
            }

            return "no token";
        }
        /// <summary>
        /// The `LoginAsync` function validates client credentials, generates a JWT token, caches it, and
        /// returns a `LoginResponse` object.
        /// </summary>
        /// <param name="LoginRequest">The `LoginAsync` method you provided is responsible for handling
        /// the login process. It takes a `LoginRequest` object as a parameter, which likely contains
        /// the client's email and password for authentication.</param>
        /// <returns>
        /// The `LoginAsync` method returns a `Task<LoginResponse>`. The `LoginResponse` object contains
        /// the following properties:
        /// - `Token`: A JWT token generated using the client's claims.
        /// - `ExpiresAt`: The expiration time of the JWT token (calculated as the current time plus 60
        /// minutes).
        /// - `Client`: An object of type `ClientInfo` containing the client's information
        /// </returns>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                const string cacheKey = "service_token";
                const string cacheU = "ClientId";

                // Call Client API to validate credentials
                var validationRequest = new ClientValidationRequest
                {
                    Email = request.Email,
                    Username = request.Username,
                    Password = request.Password
                };

                if (validationRequest.Email.IsNullOrEmpty() && validationRequest.Username.IsNullOrEmpty())
                {
                    throw new UnauthorizedAccessException("Username or Email missing for login");
                }

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_clientApiBaseUrl}api/Client/login",
                    validationRequest);

                if (!response.IsSuccessStatusCode)
                {
                    throw new UnauthorizedAccessException("Authentication failed");
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                jsonOptions.Converters.Add(new ClaimJsonConverter());

                var validationResult = await response.Content.ReadFromJsonAsync<ClientValidationResponse>(jsonOptions);

                if (validationResult == null || !validationResult.IsValid)
                {
                    throw new UnauthorizedAccessException("Invalid credentials");
                }
                // Convert DTO claims into real Claim objects
                var claims = validationResult.Claims
                    .Select(c => new Claim(c.Type, c.Value))
                    .ToList();

                // Generate JWT token using the claims
                var jwtToken = _tokenService.GenerateJwtToken(claims);

                // Cache for 50 minutes (tokens expire in 60 minutes)
                _cache.Set(cacheKey, jwtToken, TimeSpan.FromMinutes(50));
                _cache.Set(cacheU, validationResult.Id, TimeSpan.FromMinutes(50));
                return new LoginResponse

                {
                    Token = jwtToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Should match JWT expiration
                    Client = new ClientInfo
                    {
                        Id = validationResult.Id,
                        Email = validationResult.Email,
                        Username = validationResult.Username,
                        Role = validationResult.Role,
                        FullName = validationResult.Claims
                            .FirstOrDefault(c => c.Type == "fullName")?.Value ?? ""
                    }
                };
            }
            catch (HttpRequestException)
            {
                throw new ServiceUnavailableException("Client service is currently unavailable");
            }
        }

        /// <summary>
        /// The function `ValidateTokenAsync` asynchronously validates a token and returns a
        /// `TokenValidationResponse` indicating whether the token is valid or not.
        /// </summary>
        /// <param name="token">The `ValidateTokenAsync` method takes a `token` as input parameter. This
        /// token is used to validate the client's authentication token. If the token is valid, the method
        /// returns a `TokenValidationResponse` indicating that the token is valid along with the client's
        /// claims. If the token is</param>
        /// <returns>
        /// The `ValidateTokenAsync` method returns a `Task` that will eventually contain a
        /// `TokenValidationResponse`. The `TokenValidationResponse` object contains information about
        /// whether the token is valid or not, along with any error messages if the validation process
        /// encounters an exception.
        /// </returns>
        public Task<TokenValidationResponse> ValidateTokenAsync(string token)
        {
            try
            {
                var principal = _tokenService.ValidateToken(token);

                if (principal == null)
                {
                    return Task.FromResult(new TokenValidationResponse
                    {
                        IsValid = false,
                        ErrorMessage = "Invalid or expired token"
                    });
                }

                return Task.FromResult(new TokenValidationResponse
                {
                    IsValid = true,
                    Claims = principal.Claims.ToList()
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new TokenValidationResponse
                {
                    IsValid = false,
                    ErrorMessage = $"Token validation error: {ex.Message}"
                });
            }
        }

    }
    
    /* The class ServiceUnavailableException is a custom exception in C# that represents a service
    being unavailable. */
    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException(string message) : base(message) { }
    }

}
