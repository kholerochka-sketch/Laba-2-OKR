using Karpaty.Api.DTO;
using Karpaty.Api.Filters;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Karpaty.Api.Controllers;

/// <summary>
/// Контролер для роботи з порадами для походу.
/// Читати поради може будь-хто, додавати, змінювати й видаляти - тільки адміністратор
/// (заголовок X-Admin-Key).
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
    /// Отримати список порад. Необов'язковий фільтр за категорією.
    /// </summary>
    /// <param name="category">Необов'язковий фільтр: спорядження, безпека, погода, поведінка або прибирання.
    /// Для невідомої категорії повертається порожній масив.</param>
    /// <response code="200">Список порад (може бути порожнім).</response>
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
    /// <param name="id">Ідентифікатор поради.</param>
    /// <response code="200">Пораду знайдено.</response>
    /// <response code="404">Пораду не знайдено (або некоректний формат ID).</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Tip), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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
    /// Додати нову пораду (тільки для адміністратора).
    /// </summary>
    /// <remarks>
    /// Потрібен заголовок <c>X-Admin-Key</c>.
    ///
    /// Приклад запиту:
    ///
    ///     POST /api/tips
    ///     {
    ///        "title": "Зручне взуття",
    ///        "category": "спорядження",
    ///        "text": "Беріть трекінгове взуття з протектором і вже розношене."
    ///     }
    /// </remarks>
    /// <param name="dto">Дані нової поради.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter, поле потрібне для Swagger UI).</param>
    /// <response code="201">Пораду створено.</response>
    /// <response code="400">Невалідні дані.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    [HttpPost]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(typeof(Tip), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTipDTO dto,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
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
    /// Змінити пораду за ID (тільки для адміністратора).
    /// </summary>
    /// <remarks>
    /// Потрібен заголовок <c>X-Admin-Key</c>. Можна передати одне поле або кілька,
    /// поле, якого немає в запиті, не змінюється.
    /// </remarks>
    /// <param name="id">Ідентифікатор поради.</param>
    /// <param name="dto">Нові значення полів.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter, поле потрібне для Swagger UI).</param>
    /// <response code="204">Пораду оновлено.</response>
    /// <response code="400">Невалідні дані.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    /// <response code="404">Пораду не знайдено (або некоректний формат ID).</response>
    [HttpPut("{id}")]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateTipDTO dto,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
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
    /// Видалити пораду за ID (тільки для адміністратора).
    /// </summary>
    /// <remarks>
    /// Потрібен заголовок <c>X-Admin-Key</c>.
    /// </remarks>
    /// <param name="id">Ідентифікатор поради.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter, поле потрібне для Swagger UI).</param>
    /// <response code="204">Пораду видалено.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    /// <response code="404">Пораду не знайдено (або некоректний формат ID).</response>
    [HttpDelete("{id}")]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
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