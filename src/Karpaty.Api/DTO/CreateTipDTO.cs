using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO;

public class CreateTipDTO
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string title { get; set; } = string.Empty;

    [Required]
    public string category { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 5)]
    public string text { get; set; } = string.Empty;
}