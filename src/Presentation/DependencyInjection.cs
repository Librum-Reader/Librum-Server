using System.Text;
using Application.Interfaces.Managers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Application.Managers;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repository;
using Application.Services.v1;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuthenticationManager, AuthenticationManager>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddSingleton<IBookBlobStorageManager, BookBlobStorageManager>(); 

        
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.DefaultApiVersion = ApiVersion.Default;
            options.ApiVersionReader = new HeaderApiVersionReader("X-Version");
        });

        services.AddSingleton(x => new BlobServiceClient(
                                  configuration.GetValue<string>(
                                      "AzureBlobStorageConnectionString")));
        
        services.AddLogging();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddDbContext<DataContext>(options =>
        {
            var sqliteConnection = configuration.GetConnectionString("DefaultConnection");
            if (sqliteConnection == null)
            {
                throw new InvalidDataException("Can not read 'DefaultConnection'" +
                                               "from settings file");
            }
            
            options.UseSqlServer(sqliteConnection);
        });


        return services;
    }

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        var builder = services.AddIdentityCore<User>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequiredLength = 6;
            opt.User.RequireUniqueEmail = true;
        });

        builder = new IdentityBuilder(builder.UserType, 
                                      typeof(IdentityRole), 
                                      builder.Services);
        builder.AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();
    }

    public static void ConfigureJwt(this IServiceCollection services, 
                                    IConfiguration configuration)
    {
        var secret = configuration["JWT:Secret"]!;
        if (secret.IsNullOrEmpty())
        {
            throw new InvalidDataException("Can not read 'DefaultConnection'" +
                                           "from settings file");
        }
        
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = configuration["JWT:ValidIssuer"],
                ValidAudience = configuration["JWT:ValidAudience"],
                IssuerSigningKey = signingKey
            };
        });
    }
}