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

        public async Task<IEnumerable<Place>> GetAllPlacesAsync()
        {
            return await _placeRepository.GetAllAsync();
        }

        public async Task<Place?> GetPlaceByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return await _placeRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Place>> GetPlacesByCategoryAsync(string categoryString)
        {
            if (!Enum.TryParse<PlaceCategory>(categoryString, true, out var parsedCategory) ||
                !Enum.IsDefined(typeof(PlaceCategory), parsedCategory))
            {
                var allowedValues = string.Join(", ", Enum.GetNames(typeof(PlaceCategory)));
                throw new ArgumentException($"Невідома категорія '{categoryString}'. Допустимі значення: {allowedValues}");
            }

            return await _placeRepository.GetByCategoryAsync(parsedCategory);
        }

        public async Task<Place> CreatePlaceAsync(CreatePlaceDTO dto)
        {
            // Обов'язкові поля вже перевірені валідацією DTO в контролері;
            // GetValueOrDefault не дає сервісу впасти, якщо його викликають напряму (наприклад, у юніт-тестах)
            var newPlace = new Place
            {
                name = dto.name.Trim(),
                category = dto.category.GetValueOrDefault(),
                region = dto.region.GetValueOrDefault(),
                description = dto.description.Trim(),
                difficulty = dto.difficulty.GetValueOrDefault(),
                latitude = dto.latitude.GetValueOrDefault(),
                longitude = dto.longitude.GetValueOrDefault(),
                imageUrl = dto.imageUrl?.Trim() ?? string.Empty
            };

            await _placeRepository.CreateAsync(newPlace);

            return newPlace;
        }

        public async Task<Place?> UpdatePlaceAsync(string id, UpdatePlaceDTO dto)
        {
            var existingPlace = await _placeRepository.GetByIdAsync(id);
            if (existingPlace is null) return null;

            if (!string.IsNullOrWhiteSpace(dto.name))
                existingPlace.name = dto.name.Trim();

            if (dto.category.HasValue)
                existingPlace.category = dto.category.Value;

            if (dto.region.HasValue)
                existingPlace.region = dto.region.Value;

            if (dto.description is not null)
                existingPlace.description = dto.description.Trim();

            if (dto.difficulty.HasValue)
                existingPlace.difficulty = dto.difficulty.Value;

            if (dto.latitude.HasValue)
                existingPlace.latitude = dto.latitude.Value;

            if (dto.longitude.HasValue)
                existingPlace.longitude = dto.longitude.Value;

            if (dto.imageUrl is not null)
                existingPlace.imageUrl = dto.imageUrl.Trim();

            await _placeRepository.UpdateAsync(existingPlace);

            return existingPlace;
        }

        public async Task<bool> DeletePlaceAsync(string id)
        {
            var existingPlace = await _placeRepository.GetByIdAsync(id);
            if (existingPlace is null) return false;

            await _placeRepository.DeleteAsync(id);
            return true;
        }
    }
}