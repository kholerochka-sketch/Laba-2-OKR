using Karpaty.Api.DBCommunication;
using Karpaty.Api.DBCommunication.Reposetories;
using Karpaty.Api.DBCommunication.Repositories;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.Middlewares.Karpaty.Api.Middlewares;
using Karpaty.Api.Seeder;
using Karpaty.Api.Services;
using Karpaty.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Driver;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Налаштування MongoDB ---
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("MongoDBSettings:ConnectionString")
        ?? builder.Configuration["MongoDBSettings:ConnectionString"];

    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var dbName = builder.Configuration["MongoDBSettings:DatabaseName"] ?? "Karpatyyy";

    return client.GetDatabase(dbName);
});

// --- 2. Реєстрація репозиторіїв і сервісів ---
builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();
builder.Services.AddScoped<IPlaceService, PlaceService>();

builder.Services.AddScoped<ITipRepository, TipRepository>();
builder.Services.AddScoped<ITipService, TipService>();

builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

// --- 3. Контролери та JSON ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                ? "Некоректні дані запиту"
                : e.ErrorMessage)
            .Distinct();

        return new BadRequestObjectResult(new
        {
            status = 400,
            error = string.Join("; ", errors)
        });
    };
});

// --- 4. Обмеження частоти запитів (захист від спаму) ---
var feedbackPerMinute = builder.Configuration.GetValue("RateLimiting:FeedbackPerMinute", 3);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("feedback", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = feedbackPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(
                MetadataName.RetryAfter,
                out var retryAfter))
        {
            context.HttpContext.Response.Headers["Retry-After"] =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                status = 429,
                error = "Забагато запитів. Спробуйте ще раз через хвилину."
            },
            cancellationToken);
    };
});

// --- 5. CORS і Swagger ---
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

builder.Services.AddSwaggerGen(options =>
{
    // Підтягує XML-коментарі (summary, remarks, param, response) у Swagger UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

// --- 6. Глобальна обробка помилок ---
app.UseMiddleware<Karpaty.Api.Middleware.GlobalExceptionMiddleware>();

// --- 7. Створення індексів MongoDB ---
using (var scope = app.Services.CreateScope())
{
    var placeRepository = scope.ServiceProvider
        .GetRequiredService<IPlaceRepository>();

    if (placeRepository is PlaceRepository concreteRepository)
    {
        await concreteRepository.CreateIndexesAsync();
    }
}

// --- 8. Початкові дані ---
await DBSeeder.SeedPlacesAsync(app.Services);
await DBSeeder.SeedTipsAsync(app.Services);

app.UseMiddleware<GlobalExceptionMiddleware>();

// --- 7. Middleware (порядок важливий) ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseRateLimiter();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Невідомі маршрути API -> JSON 404
app.Map("/api/{**slug}", () => Results.Json(
    new
    {
        status = 404,
        error = "Маршрут API не знайдено"
    },
    statusCode: StatusCodes.Status404NotFound));

// Усе інше - сторінка фронтенду
app.MapFallbackToFile("index.html");

app.Run();
