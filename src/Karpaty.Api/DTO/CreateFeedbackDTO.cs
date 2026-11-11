using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO
{
    /// <summary>
    /// Дані для створення відгуку.
    /// </summary>
    public class CreateFeedbackDTO
    {
        /// <summary>Ім'я автора (2-50 символів).</summary>
        /// <example>Олена</example>
        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ім'я має містити від 2 до 50 символів")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Електронна пошта автора.</summary>
        /// <example>olena@example.com</example>
        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Некоректний формат email")]
        [StringLength(100, ErrorMessage = "Email занадто довгий (максимум 100 символів)")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Оцінка від 1 до 5.</summary>
        /// <example>5</example>
        [Range(1, 5, ErrorMessage = "Оцінка має бути від 1 до 5")]
        public int Rating { get; set; }

        /// <summary>Текст відгуку (10-1000 символів).</summary>
        /// <example>Дуже сподобався маршрут до водоспаду!</example>
        [Required(ErrorMessage = "Повідомлення обов'язкове")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Повідомлення має містити від 10 до 1000 символів")]
        public string Message { get; set; } = string.Empty;
    }
}