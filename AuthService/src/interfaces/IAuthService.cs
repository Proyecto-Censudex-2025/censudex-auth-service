using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.src.models;

namespace AuthService.src.Interface
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<string> GetServiceTokenAsync();
        Task<TokenValidationResponse> ValidateTokenAsync(string token);
    }
}