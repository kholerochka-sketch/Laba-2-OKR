using Karpaty.Api.Models;

namespace Karpaty.Api.DBCommunication.Repositories.Interfaces
{
    public interface IPlaceRepository
    {
        Task CreateAsync(Place place);
        Task<List<Place>> GetAllAsync();
        Task<Place?> GetByIdAsync(string id);
        Task UpdateAsync(Place place);
        Task DeleteAsync(string id);
    }
}
