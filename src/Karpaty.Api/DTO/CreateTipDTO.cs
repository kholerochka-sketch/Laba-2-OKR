using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO;

/// <summary>
/// Дані для створення поради.
/// </summary>
public class CreateTipDTO
{
    /// <summary>Заголовок поради (2-100 символів).</summary>
    /// <example>Зручне взуття</example>
    [Required(ErrorMessage = "Заголовок обов'язковий")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Заголовок має містити від 2 до 100 символів")]
    public string title { get; set; } = string.Empty;

    /// <summary>Категорія: спорядження, безпека, погода, поведінка, прибирання.</summary>
    /// <example>спорядження</example>
    [Required(ErrorMessage = "Категорія обов'язкова")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Категорія має містити від 2 до 50 символів")]
    public string category { get; set; } = string.Empty;

    /// <summary>Текст поради (5-1000 символів).</summary>
    /// <example>Беріть трекінгове взуття з протектором і вже розношене.</example>
    [Required(ErrorMessage = "Текст поради обов'язковий")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Текст має містити від 5 до 1000 символів")]
    public string text { get; set; } = string.Empty;
}