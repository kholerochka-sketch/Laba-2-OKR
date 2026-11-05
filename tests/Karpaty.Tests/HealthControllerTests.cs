using Karpaty.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace Karpaty.Tests;

public class HealthControllerTests
{
    [Fact]
    public async Task GetHealth_WhenDatabaseAvailable_ReturnsOkWithDatabaseOk()
    {
        // Arrange
        var databaseMock = new Mock<IMongoDatabase>();

        databaseMock
            .Setup(db => db.RunCommandAsync(
                It.IsAny<Command<BsonDocument>>(),
                It.IsAny<ReadPreference>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BsonDocument { { "ok", 1 } });

        var controller = new HealthController(databaseMock.Object);

        // Act
        var result = await controller.GetHealth();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var value = okResult.Value;

        Assert.NotNull(value);

        var statusProperty = value!
            .GetType()
            .GetProperty("status");

        var databaseProperty = value
            .GetType()
            .GetProperty("database");

        Assert.Equal("ok", statusProperty?.GetValue(value));
        Assert.Equal("ok", databaseProperty?.GetValue(value));
    }

    [Fact]
    public async Task GetHealth_WhenDatabaseUnavailable_ReturnsOkWithDatabaseError()
    {
        // Arrange
        var databaseMock = new Mock<IMongoDatabase>();

        databaseMock
            .Setup(db => db.RunCommandAsync(
                It.IsAny<Command<BsonDocument>>(),
                It.IsAny<ReadPreference>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MongoException("Database unavailable"));

        var controller = new HealthController(databaseMock.Object);

        // Act
        var result = await controller.GetHealth();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var value = okResult.Value;

        Assert.NotNull(value);

        var statusProperty = value!
            .GetType()
            .GetProperty("status");

        var databaseProperty = value
            .GetType()
            .GetProperty("database");

        Assert.Equal("ok", statusProperty?.GetValue(value));
        Assert.Equal("error", databaseProperty?.GetValue(value));
    }
}
