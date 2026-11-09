using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Karpaty.Api.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly IPlaceRepository _placeRepository;
        private readonly ILogger<PlaceService> _logger;

        public PlaceService(
            IPlaceRepository placeRepository,
            ILogger<PlaceService> logger)
        {
            _placeRepository = placeRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Place>> GetAllPlacesAsync()
        {
            _logger.LogInformation("Отримання всіх місць");
            return await _placeRepository.GetAllAsync();
        }

        public async Task<Place?> GetPlaceByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Передано порожній ID місця");
                return null;
            }

            _logger.LogInformation("Отримання місця з ID {PlaceId}", id);
            return await _placeRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Place>> GetPlacesByCategoryAsync(string categoryString)
        {
            if (!Enum.TryParse<PlaceCategory>(categoryString, true, out var parsedCategory) ||
                !Enum.IsDefined(typeof(PlaceCategory), parsedCategory))
            {
                var allowedValues = string.Join(", ", Enum.GetNames(typeof(PlaceCategory)));
                _logger.LogWarning("Невідома категорія місць: {Category}", categoryString);
                throw new ArgumentException(
                    $"Невідома категорія '{categoryString}'. Допустимі значення: {allowedValues}");
            }

            _logger.LogInformation("Отримання місць за категорією {Category}", parsedCategory);
            return await _placeRepository.GetByCategoryAsync(parsedCategory);
        }

        public async Task<Place> CreatePlaceAsync(CreatePlaceDTO dto)
        {
            var allPlaces = await _placeRepository.GetAllAsync();

            if (allPlaces.Any(p => string.Equals(
                p.name, dto.name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Місце з назвою {PlaceName} вже існує", dto.name);
                throw new ArgumentException($"Місце з назвою '{dto.name}' вже існує.");
            }

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

            _logger.LogInformation("Створення нового місця {PlaceName}", newPlace.name);
            await _placeRepository.CreateAsync(newPlace);

            return newPlace;
        }

        public async Task<Place?> UpdatePlaceAsync(string id, UpdatePlaceDTO dto)
        {
            var existingPlace = await _placeRepository.GetByIdAsync(id);

            if (existingPlace is null)
            {
                _logger.LogWarning("Місце з ID {PlaceId} не знайдено для оновлення", id);
                return null;
            }

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

            _logger.LogInformation("Оновлення місця з ID {PlaceId}", id);
            await _placeRepository.UpdateAsync(existingPlace);

            return existingPlace;
        }

        public async Task<bool> DeletePlaceAsync(string id)
        {
            var existingPlace = await _placeRepository.GetByIdAsync(id);

            if (existingPlace is null)
            {
                _logger.LogWarning("Місце з ID {PlaceId} не знайдено для видалення", id);
                return false;
            }

            _logger.LogInformation("Видалення місця з ID {PlaceId}", id);
            await _placeRepository.DeleteAsync(id);

            return true;
        }
    }
}
