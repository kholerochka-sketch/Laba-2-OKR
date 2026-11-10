
using System;
using System.Threading;
using System.Threading.Tasks;
using Karpaty.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Karpaty.Tests
{
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
        public async Task GetHealth_WhenDatabaseIsAvailable_ReturnsOk()
        {
            _databaseMock
                .Setup(db => db.RunCommandAsync<BsonDocument>(
                    It.IsAny<Command<BsonDocument>>(),
                    It.IsAny<ReadPreference>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BsonDocument("ok", 1));

            var result = await _controller.GetHealth();

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);

            var response = okResult.Value;

            Assert.NotNull(response);
            Assert.Equal("ok", response.GetType().GetProperty("status")?.GetValue(response));
            Assert.Equal("ok", response.GetType().GetProperty("database")?.GetValue(response));

            _databaseMock.Verify(
                db => db.RunCommandAsync<BsonDocument>(
                    It.IsAny<Command<BsonDocument>>(),
                    It.IsAny<ReadPreference>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetHealth_WhenDatabaseIsUnavailable_ReturnsDatabaseError()
        {
            _databaseMock
                .Setup(db => db.RunCommandAsync<BsonDocument>(
                    It.IsAny<Command<BsonDocument>>(),
                    It.IsAny<ReadPreference>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("MongoDB unavailable"));

            var result = await _controller.GetHealth();

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);

            var response = okResult.Value;

            Assert.NotNull(response);
            Assert.Equal("ok", response.GetType().GetProperty("status")?.GetValue(response));
            Assert.Equal("error", response.GetType().GetProperty("database")?.GetValue(response));
        }
    }
}
