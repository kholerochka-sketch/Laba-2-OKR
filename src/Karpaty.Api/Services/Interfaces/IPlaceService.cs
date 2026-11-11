using Karpaty.Api.DTO;
using Karpaty.Api.Models;

namespace Karpaty.Api.Services.Interfaces
{
    public interface IPlaceService
    {
        Task<IEnumerable<Place>> GetAllPlacesAsync();
        Task<Place?> GetPlaceByIdAsync(string id);
        Task<IEnumerable<Place>> GetPlacesByCategoryAsync(string categoryString);
        Task<Place> CreatePlaceAsync(CreatePlaceDTO dto);
        Task<Place?> UpdatePlaceAsync(string id, UpdatePlaceDTO dto);
        Task<bool> DeletePlaceAsync(string id);
    }
}