using Karpaty.Api.DTO;
using Karpaty.Api.Models;

namespace Karpaty.Api.Services.Interfaces;

public interface ITipService
{
    Task<IEnumerable<Tip>> GetAllTipsAsync();
    Task<Tip?> GetTipByIdAsync(string id);
    Task<Tip> CreateTipAsync(CreateTipDTO dto);
    Task<Tip?> UpdateTipAsync(string id, UpdateTipDTO dto);
    Task<bool> DeleteTipAsync(string id);
}