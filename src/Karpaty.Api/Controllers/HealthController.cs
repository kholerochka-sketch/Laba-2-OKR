using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Karpaty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IMongoDatabase _database;

    public HealthController(IMongoDatabase database)
    {
        _database = database;
    }

    [HttpGet]
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