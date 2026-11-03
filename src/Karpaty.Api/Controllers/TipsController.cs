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