using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.src.Interface;

namespace AuthService.src.Middleware
{
    public class JwtBlocklistMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtBlocklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenBlocklistService tokenBlocklistService)
        {
            // Skip blocklist check for logout endpoint (allow it to process)
            if (context.Request.Path.StartsWithSegments("/api/auth/logout"))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var isBlocked = await tokenBlocklistService.IsTokenBlockedAsync(token);
                
                if (isBlocked)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { Message = "Token has been revoked" });
                    return;
                }
            }

            await _next(context);
        }
    }
}