using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO;

public class CreateTipDTO
{
    [Required]
    public string title { get; set; } = string.Empty;

    [Required]
    public string category { get; set; } = string.Empty;

    [Required]
    public string text { get; set; } = string.Empty;
}