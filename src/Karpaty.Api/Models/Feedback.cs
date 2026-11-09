namespace Karpaty.Api.Models
{
    /// <summary>
    /// Відгук відвідувача сайту.
    /// </summary>
    public class Feedback
    {
        /// <summary>Унікальний ідентифікатор відгуку.</summary>
        /// <example>6720f1b2c3d4e5f6a7b8c9d2</example>
        public string? Id { get; set; }

        /// <summary>Ім'я автора.</summary>
        /// <example>Олена</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>Електронна пошта автора (бачить лише адміністратор).</summary>
        /// <example>olena@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>Оцінка від 1 до 5.</summary>
        /// <example>5</example>
        public int Rating { get; set; }

        /// <summary>Текст відгуку.</summary>
        /// <example>Дуже сподобався маршрут до водоспаду!</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>Дата створення (UTC).</summary>
        /// <example>2026-11-04T10:15:30Z</example>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}