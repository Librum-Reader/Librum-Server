using Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.BackgroundServices;

public class DeleteUnconfirmedUsers : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public DeleteUnconfirmedUsers(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetService<IUserRepository>();
                await userRepository.DeleteUnconfirmedUsers();
            }
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}