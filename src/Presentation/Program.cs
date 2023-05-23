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

    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    loggerFactory.AddFile(Directory.GetCurrentDirectory() + "/Data/Logs/");
}



// Http pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("corspolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();