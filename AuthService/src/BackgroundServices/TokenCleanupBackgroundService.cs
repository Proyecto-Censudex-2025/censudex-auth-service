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
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupBackgroundService> _logger;

        public TokenCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

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