
using Karpaty.Api.DTO;
using Karpaty.Api.Filters;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Karpaty.Api.Controllers;

/// <summary>
/// Контролер для роботи з туристичними місцями Карпат.
/// Читати місця може будь-хто, додавати, змінювати й видаляти - тільки адміністратор
/// (заголовок X-Admin-Key).
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
    /// Отримати список усіх місць або відфільтрувати за категорією.
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
        {
            return Ok(places);
        }

        if (!AllowedCategories.Contains(category))
        {
            return BadRequest(new
            {
                status = 400,
                error = "Невідома категорія. Дозволені значення: waterfall, viewpoint, meadow"
            });
        }

        var filtered = places.Where(p =>
            string.Equals(
                p.category.ToString(),
                category,
                StringComparison.OrdinalIgnoreCase));

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
        var place = await FindPlaceAsync(id);
        if (place is null)
        {
            return NotFound(new { status = 404, error = "Місце не знайдено" });
        }

        return Ok(place);
    }

    /// <summary>
    /// Додати нове місце (тільки для адміністратора).
    /// </summary>
    /// <remarks>
    /// Потрібен заголовок <c>X-Admin-Key</c>. Назва має бути унікальною (без урахування регістру).
    ///
    /// Приклад запиту:
    ///
    ///     POST /api/places
    ///     {
    ///        "name": "Водоспад Труфанець",
    ///        "category": "waterfall",
    ///        "region": "Західні_Карпати",
    ///        "description": "Мальовничий каскадний водоспад серед лісу.",
    ///        "difficulty": "easy",
    ///        "latitude": 48.5,
    ///        "longitude": 24.0,
    ///        "imageUrl": "https://example.com/trufanets.jpg"
    ///     }
    /// </remarks>
    /// <param name="dto">Дані нового місця.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter, поле потрібне для Swagger UI).</param>
    /// <response code="201">Місце створено, у відповіді створений запис і заголовок Location.</response>
    /// <response code="400">Невалідні дані або місце з такою назвою вже існує.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    [HttpPost]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(typeof(Place), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePlaceDTO dto,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
    {
        if (await NameExistsAsync(dto.name, exceptId: null))
        {
            return BadRequest(new { status = 400, error = "Місце з такою назвою вже існує" });
        }

        var createdPlace = await _placeService.CreatePlaceAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdPlace.id }, createdPlace);
    }

    /// <summary>
    /// Змінити місце за ID (тільки для адміністратора).
    /// </summary>
    /// <remarks>
    /// Потрібен заголовок <c>X-Admin-Key</c>. Можна передати одне поле або кілька,
    /// поле, якого немає в запиті, не змінюється.
    ///
    /// Приклад запиту:
    ///
    ///     PUT /api/places/{id}
    ///     {
    ///        "description": "Оновлений опис місця.",
    ///        "difficulty": "medium"
    ///     }
    /// </remarks>
    /// <param name="id">Ідентифікатор місця.</param>
    /// <param name="dto">Нові значення полів.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter, поле потрібне для Swagger UI).</param>
    /// <response code="204">Місце оновлено.</response>
    /// <response code="400">Невалідні дані, не передано жодного поля або така назва вже зайнята.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    /// <response code="404">Місце не знайдено (або некоректний формат ID).</response>
    [HttpPut("{id}")]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdatePlaceDTO dto,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
    {
        if (dto.name is null && dto.category is null && dto.region is null
            && dto.description is null && dto.difficulty is null
            && dto.latitude is null && dto.longitude is null && dto.imageUrl is null)
        {
            return BadRequest(new { status = 400, error = "Потрібно передати хоча б одне поле для оновлення" });
        }

        var existing = await FindPlaceAsync(id);
        if (existing is null)
        {
            return NotFound(new { status = 404, error = "Місце не знайдено" });
        }

        if (!string.IsNullOrWhiteSpace(dto.name) && await NameExistsAsync(dto.name, exceptId: existing.id))
        {
            return BadRequest(new { status = 400, error = "Місце з такою назвою вже існує" });
        }

        await _placeService.UpdatePlaceAsync(id, dto);
        return NoContent();
    }

    /// <summary>
    /// Видалити місце за ID (тільки для адміністратора).
    /// </summary>
    /// <remarks>
    /// Потрібен заголовок <c>X-Admin-Key</c>.
    /// </remarks>
    /// <param name="id">Ідентифікатор місця.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter, поле потрібне для Swagger UI).</param>
    /// <response code="204">Місце видалено.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    /// <response code="404">Місце не знайдено (або некоректний формат ID).</response>
    [HttpDelete("{id}")]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
    {
        var existing = await FindPlaceAsync(id);
        if (existing is null)
        {
            return NotFound(new { status = 404, error = "Місце не знайдено" });
        }

        await _placeService.DeletePlaceAsync(id);
        return NoContent();
    }

    // Шукає місце; некоректний формат ID трактуємо як "не знайдено"
    private async Task<Place?> FindPlaceAsync(string id)
    {
        try
        {
            return await _placeService.GetPlaceByIdAsync(id);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    // Перевірка дубля назви без урахування регістру; exceptId виключає саме це місце при редагуванні
    private async Task<bool> NameExistsAsync(string name, string? exceptId)
    {
        var all = await _placeService.GetAllPlacesAsync();
        return all.Any(p =>
            p.id != exceptId &&
            string.Equals(p.name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
