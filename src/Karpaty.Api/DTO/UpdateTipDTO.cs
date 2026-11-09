using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO;

/// <summary>
/// Дані для зміни поради. Передайте одне поле або кілька, відсутнє поле не змінюється.
/// </summary>
public class UpdateTipDTO
{
    /// <summary>Новий заголовок (2-100 символів).</summary>
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Заголовок має містити від 2 до 100 символів")]
    public string? title { get; set; }

    /// <summary>Нова категорія.</summary>
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Категорія має містити від 2 до 50 символів")]
    public string? category { get; set; }

    /// <summary>Новий текст (5-1000 символів).</summary>
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Текст має містити від 5 до 1000 символів")]
    public string? text { get; set; }
}