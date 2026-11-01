using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;

namespace Karpaty.Api.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly IPlaceRepository _placeRepository;

        public PlaceService(IPlaceRepository placeRepository)
        {
            _placeRepository = placeRepository;
        }

        public async Task<IEnumerable<Models.Place>> GetAllPlacesAsync()
        {
            return await _placeRepository.GetAllAsync();
        }

        public async Task<Models.Place> GetPlaceByIdAsync(string id)
        {
            return await _placeRepository.GetByIdAsync(id);
        }

        public async Task<Place> CreatePlaceAsync(CreatePlaceDTO dto)
        {
            var newPlace = new Place
            {
                name = dto.name,
                category = dto.category,
                region = dto.region,
                description = dto.description,
                difficulty = dto.difficulty,
                latitude = dto.latitude,
                longitude = dto.longitude,
                imageUrl = dto.imageUrl
            };

            await _placeRepository.CreateAsync(newPlace);

            return newPlace;
        }

        public async Task<Models.Place> UpdatePlaceAsync(string id, UpdatePlaceDTO dto)
        {
            var existingPlace = await _placeRepository.GetByIdAsync(id);
            if (existingPlace == null) return null;

            if (dto.description != null)
                existingPlace.description = dto.description;

            if (dto.imageUrl != null)
                existingPlace.imageUrl = dto.imageUrl;

            await _placeRepository.UpdateAsync(existingPlace);

            return existingPlace;
        }

        public async Task<bool> DeletePlaceAsync(string id)
        {
            var existingPlace = await _placeRepository.GetByIdAsync(id);
            if (existingPlace == null) return false;

            await _placeRepository.DeleteAsync(id);
            return true;
        }
    }
}