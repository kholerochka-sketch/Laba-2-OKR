namespace Karpaty.Api.DTO
{
    public class UpdateFeedbackDTO
    {
        public int Rating { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
