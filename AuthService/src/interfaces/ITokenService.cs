using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthService.src.models;

namespace AuthService.src.Interface
{
    public interface ITokenService
    {
        string GenerateJwtToken(List<Claim> claims);
        ClaimsPrincipal? ValidateToken(string token);
        bool IsTokenValid(string token);
    }
}