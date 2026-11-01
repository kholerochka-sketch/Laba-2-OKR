using Karpaty.Api.Models;
using Karpaty.Api.DTO;
namespace Karpaty.Api.Services.Interfaces
{
    public interface IPlaceService
    {
        Task<IEnumerable<Place>> GetAllPlacesAsync();
        Task<Place> GetPlaceByIdAsync(string id);
        Task<Place> CreatePlaceAsync(CreatePlaceDTO dto);
        Task<Place> UpdatePlaceAsync(string id, UpdatePlaceDTO dto);
        Task<bool> DeletePlaceAsync(string id);
    }
}
