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
            ["waterfall"] = PlaceCategory.Водоспад,
            ["viewpoint"] = PlaceCategory.Оглядовий_Майданчик,
            ["meadow"] = PlaceCategory.Полонина
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