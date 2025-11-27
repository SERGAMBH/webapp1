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


// Database context
builder.Services.AddDbContext<TodoContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PostgresConnectionUsers");
    Console.WriteLine($"Using connection string: {connectionString}");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    });
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

    for (var retry = 0; retry < 10; retry++)
    {
        try
        {
            logger.LogInformation($"Database connection attempt {retry + 1}/10");

            // Проверяем подключение к базе
            var canConnect = await context.Database.CanConnectAsync();
            if (canConnect)
            {
                logger.LogInformation("Database connection successful");

                // Проверяем существование таблицы
                var tableExists = await context.Database.SqlQueryRaw<bool>(
                    "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'items')").FirstOrDefaultAsync();

                logger.LogInformation($"Table 'items' exists: {tableExists}");

                if (!tableExists)
                {
                    logger.LogInformation("Creating table 'items'...");

                    // Создаем таблицу
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE items (
                            name VARCHAR(100) PRIMARY KEY
                        )");

                    logger.LogInformation("Table 'items' created successfully");

                    // Проверяем создание
                    tableExists = await context.Database.SqlQueryRaw<bool>(
                        "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'items')").FirstOrDefaultAsync();

                    logger.LogInformation($"Table creation verified: {tableExists}");

                    if (tableExists)
                    {
                        logger.LogInformation("Database initialization completed successfully");
                        return;
                    }
                }
                else
                {
                    logger.LogInformation("Table already exists, initialization completed");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Attempt {retry + 1}/10 failed: {ex.Message}");
            if (retry == 9)
            {
                logger.LogError("All database connection attempts failed");
                throw;
            }
            await Task.Delay(5000);
        }
    }
}