using Application.Common.Middleware;
using Application.Interfaces.Repositories;
using Stripe;
using Azure.Identity;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Presentation;
using Product = Domain.Entities.Product;


var builder = WebApplication.CreateBuilder(args);


// Add AzureKeyVault and Stripe as configuration provider if not self-hosted
if (builder.Configuration["LIBRUM_SELFHOSTED"] != "true")
{
    var keyVaultUrl = new Uri(builder.Configuration.GetSection("AzureKeyVaultUri").Value!);
    var azureCredential = new DefaultAzureCredential();
    builder.Configuration.AddAzureKeyVault(keyVaultUrl, azureCredential);

    StripeConfiguration.ApiKey = builder.Configuration["StripeSecretKey"];
}
else
{
    Console.WriteLine("Running in selfhosted mode, skipping AzureKeyVault and Stripe configuration");
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
	
	// Initialize the local database if self-hosted
	if (builder.Configuration["LIBRUM_SELFHOSTED"] == "true"){
		var context = services.GetRequiredService<DataContext>();
    	context.Database.EnsureCreated();
	}
	
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

    if (builder.Configuration["LIBRUM_SELFHOSTED"] == "true")
    {
        await SeedWithSelfHostedProduct(services);
    }

    await SeedWithAdminUser(services);
}



// Http pipeline
if (builder.Configuration["LIBRUM_SELFHOSTED"] == "true"){
    app.MapGet("/", () => "Librum-Server is not a web application, so it's not supposed to have a main page. <br>This page is just for health-status checking.");
}
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
    string name = "Admin";
    string email = config["AdminEmail"];
    string password = config["AdminPassword"];

    var isSelfHosted = config["LIBRUM_SELFHOSTED"] == "true";

    var userManager = services.GetRequiredService<UserManager<User>>();
    if (await userManager.FindByEmailAsync(email) == null)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            UserName = email,
            AccountCreation = DateTime.UtcNow,
            ProductId = isSelfHosted ? "1" : null,
        };

        await userManager.CreateAsync(user, password);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await userManager.ConfirmEmailAsync(user, token);
    }
}

async Task SeedWithSelfHostedProduct(IServiceProvider services)
{
    var product = new Product
    {
        ProductId = "1",
        Name = "SelfHosted",
        Description = "Dummy self hosted product",
        Price = 0,
        PriceId = "0",
        BookStorageLimit = long.MaxValue,
        AiRequestLimit = int.MaxValue,
        LiveMode = true
    };

    var productRepo = services.GetRequiredService<IProductRepository>();
    var existingProduct = await productRepo.GetByIdAsync(product.ProductId);
    if (existingProduct == default)
    {
        productRepo.CreateProduct(product);
    }
}