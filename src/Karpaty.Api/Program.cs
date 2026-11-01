using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// --- Частина Backend 1 (реєстрація MongoDB) ---
// (Ви маєте домовитися, щоб ці рядки були в Program.cs, інакше HealthController не зможе зробити ping)
builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var connectionString = builder.Configuration["MongoDbSettings:ConnectionString"] ?? "mongodb://localhost:27017";
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var dbName = builder.Configuration["MongoDbSettings:DatabaseName"] ?? "Karpaty";
    return client.GetDatabase(dbName);
});
// ----------------------------------------------

// Твоя частина Backend 2: Контролери, CORS, Swagger
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Підключення Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

// Налаштування для роздачі статики (фронтенду)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Обробка невідомих маршрутів API (щоб повертався JSON, а не index.html)
app.Map("/api/{**slug}", () => Results.Json(
    new { status = 404, error = "Маршрут API не знайдено" },
    statusCode: StatusCodes.Status404NotFound));

// Якщо маршрут не починається з /api/, віддаємо сторінку фронтенду
app.MapFallbackToFile("index.html");

app.Run();