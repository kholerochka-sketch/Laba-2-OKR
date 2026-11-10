using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Karpaty.Api.Services
{
    public class TipService : ITipService
    {
        private readonly ITipRepository _tipRepository;
        private readonly ILogger<TipService> _logger;

        public TipService(
            ITipRepository tipRepository,
            ILogger<TipService> logger)
        {
            _tipRepository = tipRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Tip>> GetAllTipsAsync()
        {
            _logger.LogInformation("Отримання всіх порад");
            return await _tipRepository.GetAllAsync();
        }

        public async Task<Tip?> GetTipByIdAsync(string id)
        {
            _logger.LogInformation("Отримання поради з ID {TipId}", id);
            return await _tipRepository.GetByIdAsync(id);
        }

        public async Task<Tip> CreateTipAsync(CreateTipDTO dto)
        {
            var tip = new Tip
            {
                title = dto.title.Trim(),
                category = dto.category.Trim().ToLowerInvariant(),
                text = dto.text.Trim()
            };

            _logger.LogInformation("Створення поради {TipTitle}", tip.title);
            await _tipRepository.CreateAsync(tip);

            return tip;
        }

        public async Task<Tip?> UpdateTipAsync(string id, UpdateTipDTO dto)
        {
            var existing = await _tipRepository.GetByIdAsync(id);

            if (existing is null)
            {
                _logger.LogWarning("Пораду з ID {TipId} не знайдено для оновлення", id);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(dto.title))
                existing.title = dto.title.Trim();

            if (!string.IsNullOrWhiteSpace(dto.category))
                existing.category = dto.category.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(dto.text))
                existing.text = dto.text.Trim();

            _logger.LogInformation("Оновлення поради з ID {TipId}", id);
            await _tipRepository.UpdateAsync(existing);

            return existing;
        }

        public async Task<bool> DeleteTipAsync(string id)
        {
            var deleted = await _tipRepository.DeleteAsync(id);

            if (deleted)
                _logger.LogInformation("Видалено пораду з ID {TipId}", id);
            else
                _logger.LogWarning("Не вдалося знайти пораду з ID {TipId} для видалення", id);

            return deleted;
        }
    }
}
