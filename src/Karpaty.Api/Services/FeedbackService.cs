using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;

namespace Karpaty.Api.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
        {
            return await _feedbackRepository.GetAllAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(string id)
        {
            return await _feedbackRepository.GetByIdAsync(id);
        }

        public async Task<Feedback> CreateFeedbackAsync(CreateFeedbackDTO dto)
        {
            var feedback = new Feedback
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                Rating = dto.Rating,
                Message = dto.Message,
            };

            await _feedbackRepository.CreateAsync(feedback);
            return feedback;
        }

        public async Task<Feedback?> UpdateFeedbackAsync(string id, UpdateFeedbackDTO dto)
        {
            var existing = await _feedbackRepository.GetByIdAsync(id);
            if (existing is null) return null;

            if (dto.Rating != null)
                existing.Rating = dto.Rating;

            if (!string.IsNullOrWhiteSpace(dto.Message))
                existing.Message = dto.Message.Trim().ToLowerInvariant();


            await _feedbackRepository.UpdateAsync(existing);
            return existing;
        }

        public async Task<bool> DeleteFeedbackAsync(string id)
        {
            return await _feedbackRepository.DeleteAsync(id);
        }
    }
}
