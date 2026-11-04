using System.ComponentModel.DataAnnotations;

namespace Karpaty.Api.DTO
{
    public class CreateFeedbackDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int Rating { get; set; }

        public string Message { get; set; } = string.Empty;

    }
}
