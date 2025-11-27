using Microsoft.EntityFrameworkCore;
using webapi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<webapi.Interfaces.ITodoRepository, webapi.Services.TodoRepository>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnectionUsers")));


// Database context - УПРОЩЕННАЯ версия
builder.Services.AddDbContext<TodoContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PostgresConnectionUsers");
    Console.WriteLine($"Using connection string: {connectionString}");
    options.UseNpgsql(connectionString);
});



var app = builder.Build();
/*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    context.Database.EnsureCreated(); 
}
*/

// Database initialization
await InitializeDatabase(app);



// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
//app.UseForwardedHeaders();

app.UseAuthorization();

app.MapControllers();

app.Run();


async Task InitializeDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<TodoContext>();

    logger.LogInformation("Starting database initialization...");

    try
    {
        // ПРОСТО создаем таблицу если её нет
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database ensure completed");
    }
    catch (Exception ex)
    {
        logger.LogError($"Database initialization failed: {ex.Message}");
    }
}