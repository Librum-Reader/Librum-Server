using Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.BackgroundServices;

public class ResetTranslationsCount : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ResetTranslationsCount(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            // Execute once at 00:00 UTC
            int hourSpan = 24 - DateTime.UtcNow.Hour;
            int numberOfHours = hourSpan;
            if (hourSpan == 24)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var logger = scope.ServiceProvider
                        .GetService<ILogger<ResetTranslationsCount>>();
                    logger.LogWarning("Resetting Translations Counts");

                    var userRepository =
                        scope.ServiceProvider.GetService<IUserRepository>();
                    await userRepository.ResetTranslationsCount();

                    numberOfHours = 24;
                }
            }

            await Task.Delay(TimeSpan.FromHours(numberOfHours), stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested);
    }
}