
using Karpaty.Api.Services;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Xunit;

namespace Karpaty.Tests
{
    public class PlaceServiceTests
    {
        private readonly FakePlaceRepository _repository;
        private readonly PlaceService _service;

        public PlaceServiceTests()
        {
            _repository = new FakePlaceRepository();
            _service = new PlaceService(_repository);
        }

        // 1. Перевірка отримання всіх місць
        [Fact]
        public async Task GetAllPlacesAsync_ReturnsAllPlaces()
        {
            var place = CreateTestPlace();
            _repository.Places.Add(place);

            var result = await _service.GetAllPlacesAsync();

            Assert.Single(result);
            Assert.Equal(place.name, result.First().name);
        }

        // 2. Перевірка отримання місця за ID
        [Fact]
        public async Task GetPlaceByIdAsync_ReturnsPlace()
        {
            var place = CreateTestPlace();
            _repository.Places.Add(place);

            var result = await _service.GetPlaceByIdAsync(place.id);

            Assert.NotNull(result);
            Assert.Equal(place.id, result.id);
        }

        // 3. Перевірка створення місця
        [Fact]
        public async Task CreatePlaceAsync_CreatesPlace()
        {
            var dto = CreateTestDTO();

            var result = await _service.CreatePlaceAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.name, result.name);
            Assert.Equal(dto.description, result.description);
            Assert.Equal(dto.imageUrl, result.imageUrl);
        }

        // 4. Перевірка виклику репозиторію при створенні
        [Fact]
        public async Task CreatePlaceAsync_CallsRepository()
        {
            var dto = CreateTestDTO();

            await _service.CreatePlaceAsync(dto);

            Assert.True(_repository.CreateCalled);
        }

        // 5. Перевірка оновлення місця
        [Fact]
        public async Task UpdatePlaceAsync_UpdatesPlace()
        {
            var place = CreateTestPlace();
            _repository.Places.Add(place);

            var dto = new UpdatePlaceDTO
            {
                description = "Нове описання",
                imageUrl = "new-image.jpg"
            };

            var result = await _service.UpdatePlaceAsync(place.id, dto);

            Assert.Equal("Нове описання", result.description);
            Assert.Equal("new-image.jpg", result.imageUrl);
            Assert.True(_repository.UpdateCalled);
        }

        // 6. Перевірка оновлення неіснуючого місця
        [Fact]
        public async Task UpdatePlaceAsync_ReturnsNull_WhenPlaceNotFound()
        {
            var dto = new UpdatePlaceDTO
            {
                description = "Нове описання",
                imageUrl = "new-image.jpg"
            };

            var result = await _service.UpdatePlaceAsync("unknown-id", dto);

            Assert.Null(result);
            Assert.False(_repository.UpdateCalled);
        }

        // 7. Перевірка видалення місця
        [Fact]
        public async Task DeletePlaceAsync_DeletesPlace()
        {
            var place = CreateTestPlace();
            _repository.Places.Add(place);

            var result = await _service.DeletePlaceAsync(place.id);

            Assert.True(result);
            Assert.True(_repository.DeleteCalled);
            Assert.Empty(_repository.Places);
        }

        // 8. Перевірка видалення неіснуючого місця
        [Fact]
        public async Task DeletePlaceAsync_ReturnsFalse_WhenPlaceNotFound()
        {
            var result = await _service.DeletePlaceAsync("unknown-id");

            Assert.False(result);
            Assert.False(_repository.DeleteCalled);
        }

        // Тестові дані
        private static Place CreateTestPlace()
        {
            return new Place
            {
                id = "test-1",
                name = "Водоспад Шипіт",
                category = PlaceCategory.Водоспад,
                region = PlaceRegion.Західні_Карпати,
                description = "Гарний водоспад",
                difficulty = PlaceDifficulty.Легко,
                latitude = 48.65f,
                longitude = 23.35f,
                imageUrl = "shipit.jpg"
            };
        }

        private static CreatePlaceDTO CreateTestDTO()
        {
            return new CreatePlaceDTO
            {
                name = "Водоспад Шипіт",
                category = PlaceCategory.Водоспад,
                region = PlaceRegion.Західні_Карпати,
                description = "Гарний водоспад",
                difficulty = PlaceDifficulty.Легко,
                latitude = 48.65f,
                longitude = 23.35f,
                imageUrl = "shipit.jpg"
            };
        }
    }

    // Підробний репозиторій замість MongoDB
    public class FakePlaceRepository : IPlaceRepository
    {
        public List<Place> Places { get; } = new();

        public bool CreateCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public Task CreateAsync(Place place)
        {
            CreateCalled = true;
            Places.Add(place);
            return Task.CompletedTask;
        }

        public Task<List<Place>> GetAllAsync()
        {
            return Task.FromResult(Places);
        }

        public Task<Place?> GetByIdAsync(string id)
        {
            var place = Places.FirstOrDefault(p => p.id == id);
            return Task.FromResult(place);
        }

        public Task UpdateAsync(Place place)
        {
            UpdateCalled = true;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            DeleteCalled = true;
            Places.RemoveAll(p => p.id == id);
            return Task.CompletedTask;
        }
    }
}
