using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Karpaty.Api.Filters;

/// <summary>
/// Фільтр авторизації для адмін-методів: вимагає заголовок <c>X-Admin-Key</c>
/// зі значенням із налаштування <c>AdminKey</c>. Без дійсного ключа повертає 401.
/// Підключається атрибутом <c>[TypeFilter(typeof(AdminKeyFilter))]</c>.
/// </summary>
public class AdminKeyFilter : IAuthorizationFilter
{
    /// <summary>Назва HTTP-заголовка з адмін-ключем.</summary>
    public const string HeaderName = "X-Admin-Key";

    private readonly string? _adminKey;

    /// <summary>
    /// Ініціалізує новий екземпляр <see cref="AdminKeyFilter"/>.
    /// </summary>
    public AdminKeyFilter(IConfiguration configuration)
    {
        _adminKey = configuration["AdminKey"];
    }

    /// <inheritdoc />
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (IsKeyValid(provided))
        {
            return;
        }

        context.Result = new JsonResult(new
        {
            status = 401,
            error = "Потрібен дійсний заголовок X-Admin-Key"
        })
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }

    private bool IsKeyValid(string provided)
    {
        // Ключ не налаштований або залишилась заглушка з репозиторію - доступ закритий для всіх
        if (string.IsNullOrWhiteSpace(_adminKey)
            || _adminKey.StartsWith("ЗАМІНИТИ", StringComparison.Ordinal)
            || string.IsNullOrEmpty(provided))
        {
            return false;
        }

        // Порівняння за сталий час, щоб ключ не можна було підібрати за часом відповіді
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(_adminKey),
            Encoding.UTF8.GetBytes(provided));
    }
}