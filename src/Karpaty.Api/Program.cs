using Karpaty.Api.DBCommunication;
// Додайте ваші using для сервісів та репозиторіїв, якщо вони в інших папках
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DBCommunication.Reposetories;
using Karpaty.Api.Services;
using Karpaty.Api.Services.Interfaces;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Налаштування бази даних MongoDB ---
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDBSettings:ConnectionString")
                           ?? builder.Configuration["MongoDBSettings:ConnectionString"];

    return new MongoClient(connectionString);
});

// Залишаємо ТІЛЬКИ ОДНУ реєстрацію бази даних (Singleton - це правильна практика для MongoDB)
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    // Беремо назву бази з конфігурації. Якщо її там немає - використовуємо дефолтну "Karpatyyy"
    var dbName = builder.Configuration["MongoDBSettings:DatabaseName"] ?? "Karpatyyy";
    return client.GetDatabase(dbName);
});

// --- 2. Реєстрація ваших власних сервісів (ОБОВ'ЯЗКОВО!) ---
builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
// Якщо додасте DbSeeder, не забудьте його тут також викликати після builder.Build()

// --- 3. Налаштування API: Контролери, CORS, Swagger ---
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

// --- 4. Налаштування Middleware (конвеєра запитів) ---

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