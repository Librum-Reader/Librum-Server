using Infrastructure.Persistence;
using Presentation;


var builder = WebApplication.CreateBuilder(args);



// Services
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();



// Startup action
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    loggerFactory.AddFile(Directory.GetCurrentDirectory() + "/Data/Logs/");

    await DataContextSeeding.SeedDataContext(services.GetRequiredService<DataContext>());
}



// Http pipeline
app.UseHttpsRedirection();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthorization();
app.MapControllers();
app.Run();