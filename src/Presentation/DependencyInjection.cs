using Application.Common.Interfaces;
using Infrastructure.Persistance;
using Infrastructure.Services.v1;
using Microsoft.EntityFrameworkCore;


namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserService, UserService>();
        
        services.AddLogging();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        });

        
        return services;
    }
}