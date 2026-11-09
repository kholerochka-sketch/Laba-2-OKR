using Karpaty.Api.Models;   
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Karpaty.Api.Controllers;

/// <summary>
/// Перевірка стану сервера та підключення до бази даних.
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
    /// Перевірити, що сервер працює і база даних MongoDB відповідає.
    /// </summary>
    /// <remarks>
    /// Сервер завжди відповідає 200. Поле <c>database</c> показує стан бази:
    /// <c>ok</c>, якщо ping пройшов, або <c>error</c>, якщо база недоступна.
    /// </remarks>
    /// <response code="200">Сервер працює. Дивіться поле database для стану бази.</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
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