using Presentation;


var builder = WebApplication.CreateBuilder(args);



// Services
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();



// Http pipeline
app.UseHttpsRedirection();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthorization();
app.MapControllers();
app.Run();