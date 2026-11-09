using Karpaty.Api.DTO;
using Karpaty.Api.Filters;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Karpaty.Api.Controllers;

/// <summary>
/// Контролер для відгуків відвідувачів: будь-хто може надіслати й переглянути відгуки,
/// а змінювати й видаляти відгуки може тільки адміністратор (заголовок X-Admin-Key).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="FeedbackController"/>.
    /// </summary>
    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Надіслати відгук.
    /// </summary>
    /// <remarks>
    /// Обмеження: ім'я 2-50 символів, коректний email, оцінка від 1 до 5,
    /// повідомлення 10-1000 символів. Не більше 3 запитів на хвилину з однієї IP-адреси
    /// (захист від спаму).
    ///
    /// Приклад запиту:
    ///
    ///     POST /api/feedback
    ///     {
    ///        "name": "Олена",
    ///        "email": "olena@example.com",
    ///        "rating": 5,
    ///        "message": "Дуже сподобався маршрут до водоспаду!"
    ///     }
    /// </remarks>
    /// <param name="dto">Дані відгуку.</param>
    /// <response code="201">Відгук збережено, у відповіді створений запис.</response>
    /// <response code="400">Невалідні дані. У полі error текст помилки.</response>
    /// <response code="429">Забагато запитів (ліміт 3 на хвилину). Заголовок Retry-After каже, скільки чекати.</response>
    [HttpPost]
    [EnableRateLimiting("feedback")]
    [ProducesResponseType(typeof(Feedback), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackDTO dto)
    {
        var created = await _feedbackService.CreateFeedbackAsync(dto);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>
    /// Отримати всі відгуки (доступно для всіх відвідувачів), нові першими.
    /// </summary>
    /// <response code="200">Список відгуків від найновіших до найстаріших.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Feedback>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
        return Ok(feedbacks.OrderByDescending(f => f.CreatedAt));
    }

    /// <summary>
    /// Отримати один відгук за ID (тільки для адміністратора).
    /// </summary>
    /// <param name="id">Ідентифікатор відгуку.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter).</param>
    /// <response code="200">Відгук знайдено.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    /// <response code="404">Відгук не знайдено (або некоректний формат ID).</response>
    [HttpGet("{id}")]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(typeof(Feedback), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        string id,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
    {
        var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
        if (feedback is null)
        {
            return NotFound(new { status = 404, error = "Відгук не знайдено" });
        }

        return Ok(feedback);
    }

    /// <summary>
    /// Змінити оцінку та/або текст відгуку (тільки для адміністратора).
    /// </summary>
    /// <remarks>
    /// Можна передати одне поле або обидва. Поле, якого немає в запиті, не змінюється.
    ///
    /// Приклад запиту:
    ///
    ///     PUT /api/feedback/{id}
    ///     {
    ///        "rating": 4,
    ///        "message": "Оновлений текст відгуку після модерації"
    ///     }
    /// </remarks>
    /// <param name="id">Ідентифікатор відгуку.</param>
    /// <param name="dto">Нові значення.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter).</param>
    /// <response code="204">Відгук оновлено.</response>
    /// <response code="400">Невалідні дані або не передано жодного поля.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    /// <response code="404">Відгук не знайдено (або некоректний формат ID).</response>
    [HttpPut("{id}")]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateFeedbackDTO dto,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
    {
        if (dto.Rating is null && string.IsNullOrWhiteSpace(dto.Message))
        {
            return BadRequest(new { status = 400, error = "Потрібно передати rating та/або message" });
        }

        var updated = await _feedbackService.UpdateFeedbackAsync(id, dto);
        if (updated is null)
        {
            return NotFound(new { status = 404, error = "Відгук не знайдено" });
        }

        return NoContent();
    }

    /// <summary>
    /// Видалити відгук за ID (тільки для адміністратора).
    /// </summary>
    /// <param name="id">Ідентифікатор відгуку.</param>
    /// <param name="adminKey">Адмін-ключ (перевіряє AdminKeyFilter).</param>
    /// <response code="204">Відгук видалено.</response>
    /// <response code="401">Заголовок X-Admin-Key відсутній або неправильний.</response>
    /// <response code="404">Відгук не знайдено (або некоректний формат ID).</response>
    [HttpDelete("{id}")]
    [TypeFilter(typeof(AdminKeyFilter))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string id,
        [FromHeader(Name = AdminKeyFilter.HeaderName)] string? adminKey = null)
    {
        var deleted = await _feedbackService.DeleteFeedbackAsync(id);
        if (!deleted)
        {
            return NotFound(new { status = 404, error = "Відгук не знайдено" });
        }

        return NoContent();
    }
}