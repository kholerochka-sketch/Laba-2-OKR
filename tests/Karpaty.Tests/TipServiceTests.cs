
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services;
using Moq;
using Xunit;

namespace Karpaty.Tests;

public class TipServiceTests
{
    private readonly Mock<ITipRepository> _repositoryMock;
    private readonly TipService _service;

    public TipServiceTests()
    {
        _repositoryMock = new Mock<ITipRepository>();
        _service = new TipService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllTipsAsync_ReturnsAllTips()
    {
        // Arrange
        var tips = new List<Tip>
        {
            new Tip { id = "1", title = "Tip 1", category = "travel", text = "Text 1" },
            new Tip { id = "2", title = "Tip 2", category = "safety", text = "Text 2" }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(tips);

        // Act
        var result = await _service.GetAllTipsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTipByIdAsync_WhenTipExists_ReturnsTip()
    {
        // Arrange
        var tip = new Tip
        {
            id = "1",
            title = "Travel tip",
            category = "travel",
            text = "Travel safely"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(tip);

        // Act
        var result = await _service.GetTipByIdAsync("1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Travel tip", result.title);
        _repositoryMock.Verify(r => r.GetByIdAsync("1"), Times.Once);
    }

    [Fact]
    public async Task GetTipByIdAsync_WhenTipDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("999"))
            .ReturnsAsync((Tip?)null);

        // Act
        var result = await _service.GetTipByIdAsync("999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTipAsync_TrimsAndNormalizesFields()
    {
        // Arrange
        var dto = new CreateTipDTO
        {
            title = "  Travel advice  ",
            category = "  TRAVEL  ",
            text = "  Be careful  "
        };

        Tip? createdTip = null;

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Tip>()))
            .Callback<Tip>(tip => createdTip = tip)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateTipAsync(dto);

        // Assert
        Assert.Equal("Travel advice", result.title);
        Assert.Equal("travel", result.category);
        Assert.Equal("Be careful", result.text);

        Assert.NotNull(createdTip);
        Assert.Equal("travel", createdTip.category);

        _repositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Tip>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTipAsync_WhenTipExists_UpdatesFields()
    {
        // Arrange
        var existingTip = new Tip
        {
            id = "1",
            title = "Old title",
            category = "travel",
            text = "Old text"
        };

        var dto = new UpdateTipDTO
        {
            title = "  New title  ",
            category = "  SAFETY  ",
            text = "  New text  "
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(existingTip);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Tip>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateTipAsync("1", dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New title", result.title);
        Assert.Equal("safety", result.category);
        Assert.Equal("New text", result.text);

        _repositoryMock.Verify(
            r => r.UpdateAsync(existingTip),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTipAsync_WhenTipDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync("999"))
            .ReturnsAsync((Tip?)null);

        var dto = new UpdateTipDTO
        {
            title = "New title"
        };

        // Act
        var result = await _service.UpdateTipAsync("999", dto);

        // Assert
        Assert.Null(result);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Tip>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateTipAsync_WhenFieldsAreEmpty_KeepsOldValues()
    {
        // Arrange
        var existingTip = new Tip
        {
            id = "1",
            title = "Old title",
            category = "travel",
            text = "Old text"
        };

        var dto = new UpdateTipDTO
        {
            title = " ",
            category = "",
            text = null
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync("1"))
            .ReturnsAsync(existingTip);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Tip>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateTipAsync("1", dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Old title", result.title);
        Assert.Equal("travel", result.category);
        Assert.Equal("Old text", result.text);
    }

    [Fact]
    public async Task DeleteTipAsync_WhenTipExists_ReturnsTrue()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("1"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteTipAsync("1");

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync("1"), Times.Once);
    }

    [Fact]
    public async Task DeleteTipAsync_WhenTipDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync("999"))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteTipAsync("999");

        // Assert
        Assert.False(result);
    }
}
