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

// Конфигурация для работы в контейнере
/*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // HTTP
    options.ListenAnyIP(5001, listenOptions => // HTTPS
    {
        listenOptions.UseHttps();
    });
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 5001;
});*/
/*
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
});
*/
/*
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // HTTP
    options.ListenAnyIP(443, listenOptions => // HTTPS
    {
        var certPath = "/https/aspnetapp.pfx";
        if (File.Exists(certPath))
        {
            listenOptions.UseHttps(certPath);
        }
        else
        {
            // Fallback для разработки без сертификата
            listenOptions.UseHttps();
        }
    });
});*/
/*
// ВАЖНО: Только HTTP, SSL терминация на nginx
builder.WebHost.UseUrls("http://*:8080");

// Для работы за reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
});
*/

var app = builder.Build();
/*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    context.Database.EnsureCreated(); 
}
*/

// Initialize database with error handling
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<TodoContext>();

        Console.WriteLine("Attempting to connect to database...");

        // Wait for database to be ready
        var retries = 10;
        while (retries > 0)
        {
            try
            {
                // Проверяем подключение
                var canConnect = context.Database.CanConnect();
                Console.WriteLine($"Database connection: {canConnect}");

                if (canConnect)
                {
                    // Принудительно создаем таблицы
                    context.Database.EnsureCreated();
                    Console.WriteLine("Database tables created successfully");
                    break;
                }
            }
            catch (Exception ex)
            {
                retries--;
                Console.WriteLine($"Database connection failed, retries left: {retries}. Error: {ex.Message}");
                if (retries == 0) throw;
                Thread.Sleep(5000);
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
}




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