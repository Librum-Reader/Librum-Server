var builder = WebApplication.CreateBuilder(args);



// Services
builder.Services.AddControllers();



var app = builder.Build();



// Http pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();