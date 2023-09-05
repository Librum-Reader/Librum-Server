using Application.Common.Middleware;
using Application.Interfaces.Services;
using AspNetCoreRateLimit;
using Azure.Identity;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Presentation;


var builder = WebApplication.CreateBuilder(args);


// Add environment variables for Docker configuration
builder.Configuration.AddEnvironmentVariables(prefix: "LIBRUM_");


// Add AzureKeyVault as configuration provider if not self-hosted
if (builder.Configuration["LIBRUM_SELFHOSTED"] != "true")
{
    var keyVaultUrl = new Uri(builder.Configuration.GetSection("AzureKeyVaultUri").Value!);
    var azureCredential = new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(keyVaultUrl, azureCredential);
}
else
{
    Console.WriteLine("Running in selfhosted mode, skipping AzureKeyVault configuration");
}


// Services
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.AddCors(p => p.AddPolicy("corspolicy",
                                          build =>
                                          {
                                              build.WithOrigins("*").AllowAnyMethod()
                                                  .AllowAnyHeader();
                                          }));

var app = builder.Build();



// Startup action
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Configure Logging
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    loggerFactory.AddFile(Directory.GetCurrentDirectory() + "/Data/Logs/");

    // Add Roles
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "Basic" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    await SeedWithAdminUser(services);
}



// Http pipeline
app.UseMiddleware<CustomIpRateLimitMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("corspolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.Run();



async Task SeedWithAdminUser(IServiceProvider services)
{
    var config = services.GetRequiredService<IConfiguration>();
    string firstName = "Admin";
    string lastName = "Admin";
    string email = config["AdminEmail"];
    string password = config["AdminPassword"];

    var userManager = services.GetRequiredService<UserManager<User>>();
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
            Role = "Basic",
            AccountCreation = DateTime.UtcNow
        };

        await userManager.CreateAsync(user, password);
        await userManager.AddToRoleAsync(user, "Admin");
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await userManager.ConfirmEmailAsync(user, token);
    }
}