using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO
{
    /// <summary>
    /// Дані для зміни відгуку. Передайте одне поле або обидва, відсутнє поле не змінюється.
    /// </summary>
    public class UpdateFeedbackDTO
    {
        /// <summary>Нова оцінка від 1 до 5.</summary>
        /// <example>4</example>
        [Range(1, 5, ErrorMessage = "Оцінка має бути від 1 до 5")]
        public int? Rating { get; set; }

        /// <summary>Новий текст відгуку (10-1000 символів).</summary>
        /// <example>Оновлений текст відгуку після модерації</example>
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Повідомлення має містити від 10 до 1000 символів")]
        public string? Message { get; set; }
    }
}
