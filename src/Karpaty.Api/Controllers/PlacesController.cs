using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Karpaty.Api.Controllers;

/// <summary>
/// Контролер для роботи з туристичними місцями Карпат.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlacesController : ControllerBase
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "waterfall", "viewpoint", "meadow"
    };

    private readonly IPlaceService _placeService;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="PlacesController"/>.
    /// </summary>
    public PlacesController(IPlaceService placeService)
    {
        _placeService = placeService;
    }

    /// <summary>
    /// Отримати список усіх місць або відфільтрувати за категорією (waterfall, viewpoint, meadow).
    /// </summary>
    /// <remarks>
    /// Приклад запиту з фільтрацією:
    ///
    ///     GET /api/places?category=waterfall
    ///
    /// Приклад відповіді (200 OK):
    ///
    ///     [
    ///        {
    ///           "id": "65f1a2b3c4d5e6f7a8b9c0d1",
    ///           "name": "Водоспад Шипіт",
    ///           "category": "waterfall",
    ///           "region": "Закарпатська область",
    ///           "description": "Один із наймальовничіших водоспадів Карпат.",
    ///           "difficulty": "easy",
    ///           "latitude": 48.6558,
    ///           "longitude": 23.2694,
    ///           "imageUrl": "https://images.unsplash.com/photo-1546182990-dffeafbe841d?w=800"
    ///        }
    ///     ]
    ///
    /// Приклад відповіді при невідомій категорії (400 Bad Request):
    ///
    ///     {
    ///        "status": 400,
    ///        "error": "Невідома категорія. Дозволені значення: waterfall, viewpoint, meadow"
    ///     }
    /// </remarks>
    /// <param name="category">Необов'язковий фільтр категорії: <c>waterfall</c>, <c>viewpoint</c> або <c>meadow</c>.</param>
    /// <response code="200">Список туристичних місць успішно отримано.</response>
    /// <response code="400">Передано невідоме значення параметра category.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Place>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var places = await _placeService.GetAllPlacesAsync();

        if (string.IsNullOrWhiteSpace(category))
            return Ok(places);

        var categoryMap = new Dictionary<string, PlaceCategory>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["waterfall"] = PlaceCategory.waterfall,
            ["viewpoint"] = PlaceCategory.viewpoint,
            ["meadow"] = PlaceCategory.meadow
        };

        if (!categoryMap.TryGetValue(category, out var selectedCategory))
        {
            return BadRequest(new
            {
                status = 400,
                error = "Невідома категорія. Дозволені значення: waterfall, viewpoint, meadow"
            });
        }

        var filtered = places
          .Where(p => p.category == selectedCategory)
          .ToList();

        return Ok(filtered);
    }

    /// <summary>
    /// Отримати деталі одного місця за його ID.
    /// </summary>
    /// <remarks>
    /// Приклад запиту:
    ///
    ///     GET /api/places/65f1a2b3c4d5e6f7a8b9c0d1
    ///
    /// Приклад відповіді, якщо місце не знайдено (404 Not Found):
    ///
    ///     {
    ///        "status": 404,
    ///        "error": "Місце не знайдено"
    ///     }
    /// </remarks>
    /// <param name="id">Унікальний ідентифікатор місця (24-символьний ObjectId MongoDB).</param>
    /// <response code="200">Повертає об'єкт знайденого туристичного місця.</response>
    /// <response code="404">Місце з вказаним ID не знайдено або формат ID некоректний.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Place), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var place = await _placeService.GetPlaceByIdAsync(id);
            if (place is null)
            {
                return NotFound(new { status = 404, error = "Місце не знайдено" });
            }

            return Ok(place);
        }
        catch (FormatException)
        {
            return NotFound(new { status = 404, error = "Місце не знайдено (некоректний формат ID)" });
        }
    }

    /// <summary>
    /// Додати нове місце.
    /// </summary>
    /// <remarks>
    /// Приклад запиту:
    ///
    ///     POST /api/places
    ///     {
    ///        "name": "Озеро Несамовите",
    ///        "category": "viewpoint",
    ///        "region": "Івано-Франківська область",
    ///        "description": "Високогірне озеро льодовикового походження на Чорногорі.",
    ///        "difficulty": "medium",
    ///        "latitude": 48.1225,
    ///        "longitude": 24.5394,
    ///        "imageUrl": "https://images.unsplash.com/photo-1506744038136-46273834b3fb?w=800"
    ///     }
    /// </remarks>
    /// <param name="dto">Дані для створення нового туристичного місця.</param>
    /// <response code="201">Місце успішно створено, повертає створений об'єкт.</response>
    /// <response code="400">Помилка валідації (некоректна категорія, складність або інші поля).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Place), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePlaceDTO dto)
    {
        if (!Enum.IsDefined(typeof(PlaceCategory), dto.category))
        {
            return BadRequest(new { status = 400, error = "Недопустима категорія. Дозволені: waterfall, viewpoint, meadow" });
        }

        if (!Enum.IsDefined(typeof(PlaceDifficulty), dto.difficulty))
        {
            return BadRequest(new { status = 400, error = "Недопустима складність. Дозволені: easy, medium, hard" });
        }

        try
        {
            var createdPlace = await _placeService.CreatePlaceAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdPlace.id }, createdPlace);
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = 400, error = ex.Message });
        }
    }

    /// <summary>
    /// Оновити місце за ID.
    /// </summary>
    /// <remarks>
    /// Приклад запиту:
    ///
    ///     PUT /api/places/65f1a2b3c4d5e6f7a8b9c0d1
    ///     {
    ///        "description": "Оновлений детальний опис маршруту до локації.",
    ///        "imageUrl": "https://images.unsplash.com/photo-1506744038136-46273834b3fb?w=800"
    ///     }
    /// </remarks>
    /// <param name="id">Ідентифікатор місця, яке потрібно оновити.</param>
    /// <param name="dto">Оновлені дані місця (опис та посилання на зображення).</param>
    /// <response code="204">Місце успішно оновлено.</response>
    /// <response code="400">Некоректні дані у тілі запиту.</response>
    /// <response code="404">Місце з таким ID не знайдено.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePlaceDTO dto)
    {
        try
        {
            var existing = await _placeService.GetPlaceByIdAsync(id);
            if (existing is null)
            {
                return NotFound(new { status = 404, error = "Місце не знайдено" });
            }

            await _placeService.UpdatePlaceAsync(id, dto);
            return NoContent();
        }
        catch (FormatException)
        {
            return NotFound(new { status = 404, error = "Місце не знайдено (некоректний формат ID)" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = 400, error = ex.Message });
        }
    }

    /// <summary>
    /// Частково оновити місце за ID.
    /// </summary>
    /// <remarks>
    /// Приклад запиту:
    ///
    ///     PATCH /api/places/65f1a2b3c4d5e6f7a8b9c0d1
    ///     {
    ///        "description": "Новий короткий опис локації.",
    ///        "imageUrl": "https://images.unsplash.com/photo-1546182990-dffeafbe841d?w=800"
    ///     }
    /// </remarks>
    /// <param name="id">Ідентифікатор місця.</param>
    /// <param name="dto">Поля місця для оновлення.</param>
    /// <response code="204">Місце успішно оновлено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Місце з таким ID не знайдено.</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(string id, [FromBody] UpdatePlaceDTO dto)
    {
        try
        {
            var existing = await _placeService.GetPlaceByIdAsync(id);
            if (existing is null)
            {
                return NotFound(new { status = 404, error = "Місце не знайдено" });
            }

            await _placeService.UpdatePlaceAsync(id, dto);
            return NoContent();
        }
        catch (FormatException)
        {
            return NotFound(new { status = 404, error = "Місце не знайдено (некоректний формат ID)" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = 400, error = ex.Message });
        }
    }

    /// <summary>
    /// Видалити місце за ID.
    /// </summary>
    /// <remarks>
    /// Приклад запиту:
    ///
    ///     DELETE /api/places/65f1a2b3c4d5e6f7a8b9c0d1
    /// </remarks>
    /// <param name="id">Ідентифікатор місця для видалення.</param>
    /// <response code="204">Місце успішно видалено.</response>
    /// <response code="404">Місце з таким ID не знайдено.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var existing = await _placeService.GetPlaceByIdAsync(id);
            if (existing is null)
            {
                return NotFound(new { status = 404, error = "Місце не знайдено" });
            }

            await _placeService.DeletePlaceAsync(id);
            return NoContent();
        }
        catch (FormatException)
        {
            return NotFound(new { status = 404, error = "Місце не знайдено (некоректний формат ID)" });
        }
    }
}