using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repository;
using Infrastructure.Services.v1;
using Microsoft.EntityFrameworkCore;


namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
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