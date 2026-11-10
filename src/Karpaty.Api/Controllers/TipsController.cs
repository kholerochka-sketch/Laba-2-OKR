using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Karpaty.Api.Controllers;

/// <summary>
/// Контролер для роботи з порадами для походу.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TipsController : ControllerBase
{
    private readonly ITipService _tipService;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="TipsController"/>.
    /// </summary>
    public TipsController(ITipService tipService)
    {
        _tipService = tipService;
    }

    /// <summary>
    /// Отримати список порад. Необов'язковий фільтр за категорією (?category=).
    /// </summary>
    /// <remarks>
    /// Приклад запиту:
    ///
    ///     GET /api/tips?category=safety
    ///
    /// Приклад відповіді (200 OK):
    ///
    ///     [
    ///        {
    ///           "id": "65f1a2b3c4d5e6f7a8b9c0d5",
    ///           "title": "Реєструйте маршрут у рятувальників",
    ///           "category": "safety",
    ///           "description": "Перед виходом у високогір'я повідомте ДСНС про свій маршрут."
    ///        }
    ///     ]
    /// </remarks>
    /// <param name="category">Необов'язкова назва категорії для фільтрації порад.</param>
    /// <response code="200">Повертає список усіх або відфільтрованих порад.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Tip>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var tips = await _tipService.GetAllTipsAsync();

        if (string.IsNullOrWhiteSpace(category))
        {
            return Ok(tips);
        }

        var filtered = tips.Where(t =>
            string.Equals(t.category.ToString(), category, StringComparison.OrdinalIgnoreCase));

        return Ok(filtered);
    }

    /// <summary>
    /// Отримати пораду за ID.
    /// </summary>
    /// <remarks>
    /// Приклад відповіді при відсутності запису (404 Not Found):
    ///
    ///     {
    ///        "status": 404,
    ///        "error": "Пораду не знайдено"
    ///     }
    /// </remarks>
    /// <param name="id">Унікальний ідентифікатор поради (ObjectId).</param>
    /// <response code="200">Повертає знайдену пораду.</response>
    /// <response code="404">Пораду не знайдено або передано некоректний формат ID.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Tip), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var tip = await _tipService.GetTipByIdAsync(id);
            if (tip is null)
            {
                return NotFound(new { status = 404, error = "Пораду не знайдено" });
            }

            return Ok(tip);
        }
        catch (FormatException)
        {
            return NotFound(new { status = 404, error = "Пораду не знайдено (некоректний формат ID)" });
        }
    }

    /// <summary>
    /// Додати нову пораду.
    /// </summary>
    /// <remarks>
    /// Приклад запиту:
    ///
    ///     POST /api/tips
    ///     {
    ///        "title": "Беріть дощовик замість парасолі",
    ///        "category": "gear",
    ///        "description": "У горах сильний вітер легко ламає парасолі, тому надійний мембранний дощовик є обов'язковим."
    ///     }
    /// </remarks>
    /// <param name="dto">Дані нової поради.</param>
    /// <response code="201">Пораду створено, повертає створений об'єкт.</response>
    /// <response code="400">Помилка валідації вхідних даних.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Tip), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTipDTO dto)
    {
        try
        {
            var created = await _tipService.CreateTipAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = 400, error = ex.Message });
        }
    }

    /// <summary>
    /// Оновити пораду за ID.
    /// </summary>
    /// <remarks>
    /// Приклад запиту:
    ///
    ///     PUT /api/tips/65f1a2b3c4d5e6f7a8b9c0d5
    ///     {
    ///        "description": "Оновлений текст туристичної поради."
    ///     }
    /// </remarks>
    /// <param name="id">Ідентифікатор поради.</param>
    /// <param name="dto">Оновлені дані поради.</param>
    /// <response code="204">Пораду успішно оновлено.</response>
    /// <response code="400">Некоректні вхідні дані.</response>
    /// <response code="404">Пораду не знайдено.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTipDTO dto)
    {
        try
        {
            var updated = await _tipService.UpdateTipAsync(id, dto);
            if (updated is null)
            {
                return NotFound(new { status = 404, error = "Пораду не знайдено" });
            }

            return NoContent();
        }
        catch (FormatException)
        {
            return NotFound(new { status = 404, error = "Пораду не знайдено (некоректний формат ID)" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = 400, error = ex.Message });
        }
    }

    /// <summary>
    /// Видалити пораду за ID.
    /// </summary>
    /// <param name="id">Ідентифікатор поради, яку необхідно видалити.</param>
    /// <response code="204">Пораду успішно видалено.</response>
    /// <response code="404">Пораду не знайдено.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var deleted = await _tipService.DeleteTipAsync(id);
            if (!deleted)
            {
                return NotFound(new { status = 404, error = "Пораду не знайдено" });
            }

            return NoContent();
        }
        catch (FormatException)
        {
            return NotFound(new { status = 404, error = "Пораду не знайдено (некоректний формат ID)" });
        }
    }
}