using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO;
public class UpdateTipDTO
{
    public string? title { get; set; }

    public string? category { get; set; }

    public string? text { get; set; }
}