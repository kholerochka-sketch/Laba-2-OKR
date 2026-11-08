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

        public async Task<IEnumerable<Models.Place>> GetAllPlacesAsync()
        {
            _logger.LogInformation("Запит на отримання всіх туристичних місць.");
            return await _placeRepository.GetAllAsync();
        }

        public async Task<Models.Place?> GetPlaceByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Спроба отримати місце з порожнім ID.");
                return null;
            }

            var place = await _placeRepository.GetByIdAsync(id);

            return place;
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
            _logger.LogInformation("Місце '{Name}' успішно створено з ID: {Id}", newPlace.name, newPlace.id);
            return newPlace;
        }

        public async Task<Models.Place> UpdatePlaceAsync(string id, UpdatePlaceDTO dto)
        {
            _logger.LogInformation("Спроба оновлення місця з ID: {Id}", id);
            var existingPlace = await _placeRepository.GetByIdAsync(id);
            if (existingPlace == null) 
            {
                _logger.LogWarning("Оновлення скасовано. Місце з ID '{Id}' не знайдено.", id);
                return null; 
            }

            if (dto.description != null)
                existingPlace.description = dto.description;

            if (dto.imageUrl != null)
                existingPlace.imageUrl = dto.imageUrl;

            await _placeRepository.UpdateAsync(existingPlace);
            _logger.LogInformation("Місце з ID: {Id} успішно оновлено.", id);
            return existingPlace;
        }

        public async Task<bool> DeletePlaceAsync(string id)
        {
            _logger.LogInformation("Спроба видалення місця з ID: {Id}", id);

            var existingPlace = await _placeRepository.GetByIdAsync(id);
            if (existingPlace == null) 
            {
                return false;
                _logger.LogWarning("Видалення скасовано. Місце з ID '{Id}' не знайдено.", id);
            }

            await _placeRepository.DeleteAsync(id);
            _logger.LogInformation("Місце з ID: {Id} успішно видалено.", id);
            return true;
        }
    }
}