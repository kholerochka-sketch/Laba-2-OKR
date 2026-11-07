using Karpaty.Api.DTO;
using Karpaty.Api.Models;

namespace Karpaty.Api.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<IEnumerable<Feedback>> GetAllFeedbacksAsync();
        Task<Feedback?> GetFeedbackByIdAsync(string id);
        Task<Feedback> CreateFeedbackAsync(CreateFeedbackDTO dto);
        Task<Feedback?> UpdateFeedbackAsync(string id, UpdateFeedbackDTO dto);
        Task<bool> DeleteFeedbackAsync(string id);
    }
}
