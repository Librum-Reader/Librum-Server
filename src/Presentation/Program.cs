using Application.Common.Middleware;
using Application.Interfaces.Services;
using Infrastructure.Persistence;
using Presentation;


var builder = WebApplication.CreateBuilder(args);



// Services
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);


var app = builder.Build();



// Startup action
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    loggerFactory.AddFile(Directory.GetCurrentDirectory() + "/Data/Logs/");

    await DataContextSeeding.SeedDataContext(services.GetRequiredService<DataContext>(), services.GetRequiredService<IAuthenticationService>(),
        services.GetRequiredService<IBookService>());
}



// Http pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();