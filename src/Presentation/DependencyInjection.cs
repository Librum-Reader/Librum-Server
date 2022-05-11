using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;


namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        });

        
        return services;
    }
}