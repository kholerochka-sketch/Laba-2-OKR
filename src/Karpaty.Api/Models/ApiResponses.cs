namespace Karpaty.Api.Models
{
    /// <summary>
    /// Стандартна відповідь API з помилкою (коди 400, 401, 404, 429).
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>HTTP-код помилки.</summary>
        /// <example>400</example>
        public int Status { get; set; }

        /// <summary>Текст помилки, зрозумілий користувачу.</summary>
        /// <example>Назва має містити від 2 до 100 символів</example>
        public string Error { get; set; } = string.Empty;
    }

    /// <summary>
    /// Відповідь перевірки стану сервера.
    /// </summary>
    public class HealthResponse
    {
        /// <summary>Стан застосунку.</summary>
        /// <example>ok</example>
        public string Status { get; set; } = string.Empty;

        /// <summary>Стан підключення до MongoDB: ok або error.</summary>
        /// <example>ok</example>
        public string Database { get; set; } = string.Empty;
    }
}