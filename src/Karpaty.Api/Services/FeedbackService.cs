
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Karpaty.Api.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(
            IFeedbackRepository feedbackRepository,
            ILogger<FeedbackService> logger)
        {
            _feedbackRepository = feedbackRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
        {
            _logger.LogInformation("Отримання всіх відгуків");
            return await _feedbackRepository.GetAllAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(string id)
        {
            _logger.LogInformation("Отримання відгуку з ID {FeedbackId}", id);
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

            _logger.LogInformation("Створення відгуку від {Name}", feedback.Name);
            await _feedbackRepository.CreateAsync(feedback);

            return feedback;
        }

        public async Task<Feedback?> UpdateFeedbackAsync(string id, UpdateFeedbackDTO dto)
        {
            var existing = await _feedbackRepository.GetByIdAsync(id);

            if (existing is null)
            {
                _logger.LogWarning("Відгук з ID {FeedbackId} не знайдено для оновлення", id);
                return null;
            }

            if (dto.Rating.HasValue)
                existing.Rating = dto.Rating.Value;

            if (!string.IsNullOrWhiteSpace(dto.Message))
                existing.Message = dto.Message.Trim();

            _logger.LogInformation("Оновлення відгуку з ID {FeedbackId}", id);
            await _feedbackRepository.UpdateAsync(existing);

            return existing;
        }

        public async Task<bool> DeleteFeedbackAsync(string id)
        {
            var deleted = await _feedbackRepository.DeleteAsync(id);

            if (deleted)
                _logger.LogInformation("Видалено відгук з ID {FeedbackId}", id);
            else
                _logger.LogWarning("Не вдалося знайти відгук з ID {FeedbackId} для видалення", id);

            return deleted;
        }
    }
}

