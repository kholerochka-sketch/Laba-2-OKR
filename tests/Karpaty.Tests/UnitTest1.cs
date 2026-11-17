
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Karpaty.Api.Models;
using Karpaty.Api.DTO;
using Karpaty.Api.Services;
using Karpaty.Api.Services.Interfaces;
using Karpaty.Api.Controllers;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;

namespace Karpaty.Tests
{
    // =====================================
    // PLACE SERVICE TESTS
    // =====================================

    public class PlaceServiceTests
    {
        private readonly Mock<IPlaceRepository> _repositoryMock;
        private readonly Mock<ILogger<PlaceService>> _loggerMock;
        private readonly PlaceService _service;

        public PlaceServiceTests()
        {
            // Arrange
            _repositoryMock = new Mock<IPlaceRepository>();
            _loggerMock = new Mock<ILogger<PlaceService>>();

            _service = new PlaceService(
                _repositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllPlacesAsync_ReturnsAllPlaces()
        {
            // Arrange
            var places = new List<Place>
            {
                new Place { id = "1", description = "First" },
                new Place { id = "2", description = "Second" }
            };

            _repositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(places);

            // Act
            var result = await _service.GetAllPlacesAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal(places, result);

            _repositoryMock.Verify(
                r => r.GetAllAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetPlaceByIdAsync_ReturnsPlace()
        {
            // Arrange
            var place = new Place
            {
                id = "1",
                description = "Test place"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(place);

            // Act
            var result = await _service.GetPlaceByIdAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test place", result.description);

            _repositoryMock.Verify(
                r => r.GetByIdAsync("1"),
                Times.Once);
        }

        [Fact]
        public async Task GetPlaceByIdAsync_ReturnsNullWhenNotFound()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            // Act
            var result = await _service.GetPlaceByIdAsync("999");

            // Assert
            Assert.Null(result);

            _repositoryMock.Verify(
                r => r.GetByIdAsync("999"),
                Times.Once);
        }

        [Fact]
        public async Task CreatePlaceAsync_CreatesPlace()
        {
            // Arrange
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

            // Act
            var result = await _service.CreatePlaceAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New place", result.name);
            Assert.Equal("Description", result.description);
            Assert.Equal("image.jpg", result.imageUrl);

            _repositoryMock.Verify(
                r => r.GetAllAsync(),
                Times.Once);

            _repositoryMock.Verify(
                r => r.CreateAsync(It.IsAny<Place>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePlaceAsync_UpdatesExistingPlace()
        {
            // Arrange
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

            // Act
            var result = await _service.UpdatePlaceAsync("1", dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New description", result.description);
            Assert.Equal("new.jpg", result.imageUrl);

            _repositoryMock.Verify(
                r => r.GetByIdAsync("1"),
                Times.Once);

            _repositoryMock.Verify(
                r => r.UpdateAsync(place),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePlaceAsync_ReturnsNullWhenPlaceNotFound()
        {
            // Arrange
            var dto = new UpdatePlaceDTO
            {
                description = "Updated"
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            // Act
            var result = await _service.UpdatePlaceAsync("999", dto);

            // Assert
            Assert.Null(result);

            _repositoryMock.Verify(
                r => r.GetByIdAsync("999"),
                Times.Once);

            _repositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Place>()),
                Times.Never);
        }

        [Fact]
        public async Task DeletePlaceAsync_DeletesExistingPlace()
        {
            // Arrange
            var place = new Place { id = "1" };

            _repositoryMock
                .Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(place);

            _repositoryMock
                .Setup(r => r.DeleteAsync("1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeletePlaceAsync("1");

            // Assert
            Assert.True(result);

            _repositoryMock.Verify(
                r => r.GetByIdAsync("1"),
                Times.Once);

            _repositoryMock.Verify(
                r => r.DeleteAsync("1"),
                Times.Once);
        }

        [Fact]
        public async Task DeletePlaceAsync_ReturnsFalseWhenPlaceNotFound()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            // Act
            var result = await _service.DeletePlaceAsync("999");

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(
                r => r.GetByIdAsync("999"),
                Times.Once);

            _repositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<string>()),
                Times.Never);
        }
    }

    // =====================================
    // PLACES CONTROLLER TESTS
    // =====================================

    public class PlacesControllerTests
    {
        private readonly Mock<IPlaceService> _serviceMock;
        private readonly PlacesController _controller;

        public PlacesControllerTests()
        {
            // Arrange
            _serviceMock = new Mock<IPlaceService>();
            _controller = new PlacesController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            // Arrange
            var places = new List<Place>
            {
                new Place
                {
                    id = "1",
                    description = "Test place"
                }
            };

            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(places);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Single(returnedPlaces);

            _serviceMock.Verify(
                s => s.GetAllPlacesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Empty(returnedPlaces);

            _serviceMock.Verify(
                s => s.GetAllPlacesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAll_WithWaterfallCategory_ReturnsOnlyWaterfalls()
        {
            // Arrange
            var places = new List<Place>
            {
                new Place
                {
                    id = "1",
                    name = "Водоспад Шипіт",
                    category = PlaceCategory.waterfall
                },
                new Place
                {
                    id = "2",
                    name = "Оглядовий майданчик",
                    category = PlaceCategory.viewpoint
                },
                new Place
                {
                    id = "3",
                    name = "Полонина Боржава",
                    category = PlaceCategory.meadow
                }
            };

            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync((IEnumerable<Place>)places);

            // Act
            var result = await _controller.GetAll("waterfall");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Single(returnedPlaces);

            Assert.All(
                returnedPlaces,
                p => Assert.Equal(PlaceCategory.waterfall, p.category));

            _serviceMock.Verify(
                s => s.GetAllPlacesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetAll_WithUnknownCategory_ReturnsBadRequest()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            // Act
            var result = await _controller.GetAll("unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
