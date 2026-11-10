using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Karpaty.Api.Controllers;

/// <summary>
/// Контролер для перевірки працездатності API та з'єднання з базою даних MongoDB.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IMongoDatabase _database;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="HealthController"/>.
    /// </summary>
    public HealthController(IMongoDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// Перевірити стан сервера API та підключення до бази даних MongoDB.
    /// </summary>
    /// <remarks>
    /// Виконує команду <c>ping</c> до бази даних MongoDB.
    ///
    /// Приклад успішної відповіді (коли база даних доступна):
    ///
    ///     GET /api/health
    ///     {
    ///        "status": "ok",
    ///        "database": "ok"
    ///     }
    ///
    /// Приклад відповіді (коли база даних недоступна):
    ///
    ///     GET /api/health
    ///     {
    ///        "status": "ok",
    ///        "database": "error"
    ///     }
    /// </remarks>
    /// <response code="200">Повертає поточний статус API та статус з'єднання з базою даних.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            // Робимо ping до бази даних
            await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
            return Ok(new { status = "ok", database = "ok" });
        }
        catch
        {
            // Якщо база недоступна
            return Ok(new { status = "ok", database = "error" });
        }
    }
}