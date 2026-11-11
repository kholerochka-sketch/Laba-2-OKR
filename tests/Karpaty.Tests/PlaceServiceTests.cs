using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Karpaty.Api.Models;
using Karpaty.Api.DTO;
using Karpaty.Api.Services;
using Karpaty.Api.Services.Interfaces;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;

namespace Karpaty.Tests
{
    public class PlaceServiceTests
    {
        private readonly Mock<IPlaceRepository> _repositoryMock;
        private readonly PlaceService _service;

        public PlaceServiceTests()
        {
            _repositoryMock = new Mock<IPlaceRepository>();
            _service = new PlaceService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllPlacesAsync_ReturnsAllPlaces()
        {
            var places = new List<Place>
            {
                new Place { id = "1", description = "First" },
                new Place { id = "2", description = "Second" }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(places);

            var result = await _service.GetAllPlacesAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetPlaceByIdAsync_ReturnsPlace()
        {
            var place = new Place
            {
                id = "1",
                description = "Test place"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(place);

            var result = await _service.GetPlaceByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal("Test place", result.description);
        }

        [Fact]
        public async Task GetPlaceByIdAsync_ReturnsNullWhenNotFound()
        {
            _repositoryMock
                .Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            var result = await _service.GetPlaceByIdAsync("999");

            Assert.Null(result);
        }

        [Fact]
public async Task CreatePlaceAsync_CreatesPlace()
{
    var dto = new CreatePlaceDTO
    {
        name = "New place",
        description = "Description",
        imageUrl = "image.jpg"
    };

    _repositoryMock
        .Setup(r => r.GetAllAsync())
        .ReturnsAsync(new List<Place>());

    _repositoryMock
        .Setup(r => r.CreateAsync(It.IsAny<Place>()))
        .Returns(Task.CompletedTask);

    var result = await _service.CreatePlaceAsync(dto);

            Assert.Equal("New place", result.name);
            Assert.Equal("Description", result.description);

            _repositoryMock.Verify(
                r => r.CreateAsync(It.IsAny<Place>()),
                Times.Once
            );
        }
                [Fact]
        public async Task CreatePlaceAsync_ThrowsExceptionWhenPlaceAlreadyExists()
        {
            var dto = new CreatePlaceDTO
            {
                name = "Existing place",
                description = "New description",
                imageUrl = "new.jpg"
            };

            var existingPlaces = new List<Place>
            {
                new Place
                {
                    id = "1",
                    name = "Existing place"
                }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(existingPlaces);

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePlaceAsync(dto)
            );

            Assert.Contains("Existing place", exception.Message);

            _repositoryMock.Verify(
                r => r.CreateAsync(It.IsAny<Place>()),
                Times.Never
            );
        }

        [Fact]
        public async Task CreatePlaceAsync_ThrowsExceptionWhenPlaceExistsIgnoringCase()
        {
            var dto = new CreatePlaceDTO
            {
                name = "EXISTING PLACE",
                description = "New description",
                imageUrl = "new.jpg"
            };

            var existingPlaces = new List<Place>
            {
                new Place
                {
                    id = "1",
                    name = "Existing place"
                }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(existingPlaces);

            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePlaceAsync(dto)
            );

            _repositoryMock.Verify(
                r => r.CreateAsync(It.IsAny<Place>()),
                Times.Never
            );
        }

                [Fact]
        public async Task UpdatePlaceAsync_UpdatesExistingPlace()
        {
            var place = new Place
            {
                id = "1",
                description = "Old description",
                imageUrl = "old.jpg"
            };

            var dto = new UpdatePlaceDTO
            {
                description = "New description",
                imageUrl = "new.jpg"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(place);

            _repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Place>()))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdatePlaceAsync("1", dto);

            Assert.Equal("New description", result.description);
            Assert.Equal("new.jpg", result.imageUrl);

            _repositoryMock.Verify(
                r => r.UpdateAsync(place),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdatePlaceAsync_ReturnsNullWhenPlaceNotFound()
        {
            var dto = new UpdatePlaceDTO
            {
                description = "Updated"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            var result = await _service.UpdatePlaceAsync("999", dto);

            Assert.Null(result);

            _repositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Place>()),
                Times.Never
            );
        }

        [Fact]
        public async Task DeletePlaceAsync_DeletesExistingPlace()
        {
            var place = new Place { id = "1" };

            _repositoryMock
                .Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(place);

            _repositoryMock
                .Setup(r => r.DeleteAsync("1"))
                .Returns(Task.CompletedTask);

            var result = await _service.DeletePlaceAsync("1");

            Assert.True(result);

            _repositoryMock.Verify(
                r => r.DeleteAsync("1"),
                Times.Once
            );
        }

        [Fact]
        public async Task DeletePlaceAsync_ReturnsFalseWhenPlaceNotFound()
        {
            _repositoryMock
                .Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            var result = await _service.DeletePlaceAsync("999");

            Assert.False(result);

            _repositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<string>()),
                Times.Never
            );
        }
    }
}
