using Karpaty.Api.Models;

namespace Karpaty.Api.DBCommunication.Repositories.Interfaces
{
    public interface ITipRepository
    {
        Task<List<Tip>> GetAllAsync();
        Task<Tip?> GetByIdAsync(string id);
        Task CreateAsync(Tip tip);
        Task UpdateAsync(Tip tip);
        Task<bool> DeleteAsync(string id);
    }
}