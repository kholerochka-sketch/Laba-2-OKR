
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    }

    // =====================================
    // HEALTH CONTROLLER TESTS
    // =====================================

    public class HealthControllerTests
    {
        private readonly Mock<IMongoDatabase> _databaseMock;
        private readonly HealthController _controller;

        public HealthControllerTests()
        {
            _databaseMock = new Mock<IMongoDatabase>();
            _controller = new HealthController(_databaseMock.Object);
        }

        [Fact]
        public async Task GetHealth_ReturnsOkWhenDatabaseWorks()
        {
            _databaseMock
                .Setup(d => d.RunCommandAsync<BsonDocument>(
                    It.IsAny<Command<BsonDocument>>(),
                    It.IsAny<ReadPreference>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(new BsonDocument("ok", 1));

            var result = await _controller.GetHealth();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetHealth_ReturnsOkWhenDatabaseFails()
        {
            _databaseMock
                .Setup(d => d.RunCommandAsync<BsonDocument>(
                    It.IsAny<Command<BsonDocument>>(),
                    It.IsAny<ReadPreference>(),
                    It.IsAny<CancellationToken>()
                ))
                .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetHealth();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }

    // =====================================
    // PLACE REPOSITORY TESTS
    // =====================================

    public class PlaceRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _databaseMock;
        private readonly Mock<IMongoCollection<PlaceDocument>> _collectionMock;
        private readonly PlaceRepository _repository;

        public PlaceRepositoryTests()
        {
            _databaseMock = new Mock<IMongoDatabase>();
            _collectionMock = new Mock<IMongoCollection<PlaceDocument>>();

            _databaseMock
                .Setup(d => d.GetCollection<PlaceDocument>(
                    "places",
                    It.IsAny<MongoCollectionSettings>()
                ))
                .Returns(_collectionMock.Object);

            _repository = new PlaceRepository(_databaseMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPlaces()
        {
            var documents = new List<PlaceDocument>
            {
                new PlaceDocument
                {
                    id = "1",
                    description = "First place",
                    imageUrl = "first.jpg"
                },
                new PlaceDocument
                {
                    id = "2",
                    description = "Second place",
                    imageUrl = "second.jpg"
                }
            };

            var cursorMock = new Mock<IAsyncCursor<PlaceDocument>>();

            cursorMock
                .SetupSequence(c => c.MoveNextAsync(
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            cursorMock
                .Setup(c => c.Current)
                .Returns(documents);

            _collectionMock
                .Setup(c => c.FindAsync<PlaceDocument>(
                    It.IsAny<FilterDefinition<PlaceDocument>>(),
                    It.IsAny<FindOptions<PlaceDocument, PlaceDocument>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("First place", result[0].description);
            Assert.Equal("Second place", result[1].description);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsPlace()
        {
            var document = new PlaceDocument
            {
                id = "507f1f77bcf86cd799439011",
                description = "Test place",
                imageUrl = "image.jpg"
            };

            var cursorMock = new Mock<IAsyncCursor<PlaceDocument>>();

            cursorMock
                .SetupSequence(c => c.MoveNextAsync(
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            cursorMock
                .Setup(c => c.Current)
                .Returns(new List<PlaceDocument> { document });

            _collectionMock
                .Setup(c => c.FindAsync<PlaceDocument>(
                    It.IsAny<FilterDefinition<PlaceDocument>>(),
                    It.IsAny<FindOptions<PlaceDocument, PlaceDocument>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByIdAsync(document.id);

            Assert.NotNull(result);
            Assert.Equal(document.id, result.id);
            Assert.Equal(document.description, result.description);
            Assert.Equal(document.imageUrl, result.imageUrl);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenNotFound()
        {
            var cursorMock = new Mock<IAsyncCursor<PlaceDocument>>();

            cursorMock
                .SetupSequence(c => c.MoveNextAsync(
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(false);

            cursorMock
                .Setup(c => c.Current)
                .Returns(new List<PlaceDocument>());

            _collectionMock
                .Setup(c => c.FindAsync<PlaceDocument>(
                    It.IsAny<FilterDefinition<PlaceDocument>>(),
                    It.IsAny<FindOptions<PlaceDocument, PlaceDocument>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(cursorMock.Object);

            var result = await _repository.GetByIdAsync(
                "507f1f77bcf86cd799439011"
            );

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_InsertsDocument()
        {
            var place = new Place
            {
                id = "507f1f77bcf86cd799439011",
                description = "New place",
                imageUrl = "new.jpg"
            };

            _collectionMock
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<PlaceDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()
                ))
                .Returns(Task.CompletedTask);

            await _repository.CreateAsync(place);

            _collectionMock.Verify(
                c => c.InsertOneAsync(
                    It.IsAny<PlaceDocument>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteAsync_DeletesDocument()
        {
            _collectionMock
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<PlaceDocument>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(Mock.Of<DeleteResult>());

            await _repository.DeleteAsync("507f1f77bcf86cd799439011");

            _collectionMock.Verify(
                c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<PlaceDocument>>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }
    }
}
