using Karpaty.Api.Models;

namespace Karpaty.Api.DBCommunication.Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Task CreateAsync(Feedback feedback);
        Task<List<Feedback>> GetAllAsync();
        Task<Feedback?> GetByIdAsync(string id);
        Task UpdateAsync(Feedback feedback);
        Task<bool> DeleteAsync(string id);
    }
}
