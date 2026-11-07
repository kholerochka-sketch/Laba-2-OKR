using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;

namespace Karpaty.Api.Services
{
    public class TipService : ITipService
    {
        private readonly ITipRepository _tipRepository;

        public TipService(ITipRepository tipRepository)
        {
            _tipRepository = tipRepository;
        }

        public async Task<IEnumerable<Tip>> GetAllTipsAsync()
        {
            return await _tipRepository.GetAllAsync();
        }

        public async Task<Tip?> GetTipByIdAsync(string id)
        {
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

            await _tipRepository.CreateAsync(tip);
            return tip;
        }

        public async Task<Tip?> UpdateTipAsync(string id, UpdateTipDTO dto)
        {
            var existing = await _tipRepository.GetByIdAsync(id);
            if (existing is null) return null;

            if (!string.IsNullOrWhiteSpace(dto.title))
                existing.title = dto.title.Trim();

            if (!string.IsNullOrWhiteSpace(dto.category))
                existing.category = dto.category.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(dto.text))
                existing.text = dto.text.Trim();

            await _tipRepository.UpdateAsync(existing);
            return existing;
        }

        public async Task<bool> DeleteTipAsync(string id)
        {
            return await _tipRepository.DeleteAsync(id);
        }
    }
}