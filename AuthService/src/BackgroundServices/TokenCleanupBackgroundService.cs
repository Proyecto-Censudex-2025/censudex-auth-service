using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.src.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace authservice.src.BackgroundServices
{
    public class TokenCleanupBackgroundService : BackgroundService
    {
        /* This code snippet is defining a C# class called `TokenCleanupBackgroundService` that extends
        `BackgroundService`. */
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupBackgroundService> _logger;

        public TokenCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// This C# function is an asynchronous background service that cleans up expired tokens at
        /// hourly intervals.
        /// </summary>
        /// <param name="CancellationToken">A CancellationToken is a structure that is used to propagate
        /// notification that operations should be canceled. It can be used to request that an operation
        /// be canceled.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var tokenBlocklistService = scope.ServiceProvider
                        .GetRequiredService<ITokenBlocklistService>();

                    tokenBlocklistService.CleanupExpiredTokens();
                    _logger.LogInformation("Expired tokens cleaned up at {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired tokens");
                }

                // Run cleanup every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}