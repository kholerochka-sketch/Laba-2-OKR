
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;
using Karpaty.Api.Models;
using Karpaty.Api.DTO;
using Karpaty.Api.Services;
using Karpaty.Api.Services.Interfaces;
using Karpaty.Api.Controllers;
using Karpaty.Api.DBCommunication.Documents;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DBCommunication.Reposetories;

namespace Karpaty.Tests
{
    // =====================================
    // PLACE SERVICE TESTS
    // =====================================

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

    // =====================================
    // PLACES CONTROLLER TESTS
    // =====================================

    public class PlacesControllerTests
    {
        private readonly Mock<IPlaceService> _serviceMock;
        private readonly PlacesController _controller;

        public PlacesControllerTests()
        {
            _serviceMock = new Mock<IPlaceService>();
            _controller = new PlacesController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
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

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Single(returnedPlaces);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList()
        {
            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Empty(returnedPlaces);
        }

        [Fact]
        public async Task GetAll_WithWaterfallCategory_ReturnsOnlyWaterfalls()
        {
            var places = new List<Place>
            {
                new Place
                {
                    id = "1",
                    name = "Водоспад Шипіт",
                    category = PlaceCategory.Водоспад
                },
                new Place
                {
                    id = "2",
                    name = "Оглядовий майданчик",
                    category = PlaceCategory.Оглядовий_Майданчик
                },
                new Place
                {
                    id = "3",
                    name = "Полонина Боржава",
                    category = PlaceCategory.Полонина
                }
            };

            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync((IEnumerable<Place>)places);

            // Перевіряємо, чи Mock повертає всі три місця
            var serviceResult =
                await _serviceMock.Object.GetAllPlacesAsync();

            Assert.Equal(3, serviceResult.Count());

            // Перевіряємо фільтрацію контролера
            var result = await _controller.GetAll("waterfall");

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Single(returnedPlaces);

            Assert.All(returnedPlaces, p =>
                Assert.Equal(PlaceCategory.Водоспад, p.category));
        }

        [Fact]
        public async Task GetAll_WithUnknownCategory_ReturnsBadRequest()
        {
            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            var result = await _controller.GetAll("unknown");

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
