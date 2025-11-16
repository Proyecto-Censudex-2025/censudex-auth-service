using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.src.Interface;

namespace AuthService.src.Middleware
{
    public class JwtBlocklistMiddleware
    {
        /* In the provided C# code snippet, the `private readonly RequestDelegate _next;` declaration
        and the `JwtBlocklistMiddleware` constructor `public JwtBlocklistMiddleware(RequestDelegate
        next)` are related to middleware processing in an ASP.NET Core application. */
        private readonly RequestDelegate _next;

        public JwtBlocklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// This C# function checks if a token is blocked and returns an error message if it is,
        /// otherwise it proceeds with the next step in the middleware pipeline.
        /// </summary>
        /// <param name="HttpContext">The `HttpContext` parameter represents the HTTP request and
        /// response context for an ASP.NET Core application. It provides access to information about
        /// the incoming HTTP request and allows you to generate the HTTP response.</param>
        /// <param name="ITokenBlocklistService">`ITokenBlocklistService` is an interface that likely
        /// provides methods to interact with a token blocklist service. In this context, it is used to
        /// check if a given token is blocked or revoked. The `IsTokenBlockedAsync` method is likely
        /// used to asynchronously check if a token is present</param>
        /// <returns>
        /// If the request path starts with "/api/auth/logout", the method will immediately call the
        /// `_next` delegate to allow the logout endpoint to process. Otherwise, it will check if there
        /// is an Authorization header in the request, extract the token from it, and then check if the
        /// token is blocked using the `IsTokenBlockedAsync` method from the `tokenBlocklistService`. If
        /// the token is blocked
        /// </returns>
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