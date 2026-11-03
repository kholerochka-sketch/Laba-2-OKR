using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO;
public class UpdateTipDTO
{
    [StringLength(100, MinimumLength = 2)]
    public string? title { get; set; }

    public string? category { get; set; }

    [StringLength(1000, MinimumLength = 5)]
    public string? text { get; set; }
}