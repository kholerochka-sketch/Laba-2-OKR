using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;

namespace Karpaty.Api.Services
{
    public class PlaceService : IPlaceService
    {
        private readonly IPlaceRepository _placeRepository;
        private readonly ILogger<PlaceService> _logger;
        public PlaceService(IPlaceRepository placeRepository, ILogger<PlaceService> logger)
        {
            _placeRepository = placeRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Place>> GetAllPlacesAsync()
        {
            _logger.LogInformation("Запит на отримання всіх туристичних місць.");
            return await _placeRepository.GetAllAsync();
        }

        public async Task<Place?> GetPlaceByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Спроба отримати місце з порожнім ID.");
                return null;
            }

            return await _placeRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Place>> GetPlacesByCategoryAsync(string categoryString)
        {
            if (!Enum.TryParse<PlaceCategory>(categoryString, true, out var parsedCategory) ||
                !Enum.IsDefined(typeof(PlaceCategory), parsedCategory))
            {
                _logger.LogWarning("Спроба отримати місце з невідомою категорією");
                var allowedValues = string.Join(", ", Enum.GetNames(typeof(PlaceCategory)));
                throw new ArgumentException($"Невідома категорія '{categoryString}'. Допустимі значення: {allowedValues}");
            }

            return await _placeRepository.GetByCategoryAsync(parsedCategory);
        }

        public async Task<Models.Place> CreatePlaceAsync(CreatePlaceDTO dto)
        {
            _logger.LogInformation("Спроба створення нового місця: {Name}", dto.name);
            var allPlaces = await _placeRepository.GetAllAsync();

            if (allPlaces.Any(p => string.Equals(p.name, dto.name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Створення скасовано. Місце з назвою '{Name}' вже існує.", dto.name);
                throw new ArgumentException($"Місце з назвою '{dto.name}' вже існує.");
            }

            var newPlace = new Models.Place
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
            _logger.LogInformation("Місце '{Name}' успішно створено з ID: {Id}", newPlace.name, newPlace.id);
            return newPlace;
        }

        public async Task<Place?> UpdatePlaceAsync(string id, UpdatePlaceDTO dto)
        {
            _logger.LogInformation("Спроба оновлення місця з ID: {Id}", id);
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
            _logger.LogInformation("Місце з ID: {Id} успішно оновлено.", id);
            return existingPlace;
        }

        public async Task<bool> DeletePlaceAsync(string id)
        {
            _logger.LogInformation("Спроба видалення місця з ID: {Id}", id);

            var existingPlace = await _placeRepository.GetByIdAsync(id);
            if (existingPlace is null) return false;

            await _placeRepository.DeleteAsync(id);
            _logger.LogInformation("Місце з ID: {Id} успішно видалено.", id);
            return true;
        }
    }
}