
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
        // Arrange: створення mock-репозиторію та сервісу
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
        Assert.Equal(tips, result);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllTipsAsync_WhenNoTipsExist_ReturnsEmptyCollection()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Tip>());

        // Act
        var result = await _service.GetAllTipsAsync();

        // Assert
        Assert.Empty(result);
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
        Assert.Equal("1", result.id);
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
        _repositoryMock.Verify(r => r.GetByIdAsync("999"), Times.Once);
    }

    [Theory]
    [InlineData("  Travel advice  ", "  TRAVEL  ", "  Be careful  ",
        "Travel advice", "travel", "Be careful")]
    [InlineData(" Safety rules ", " SAFETY ", " Stay alert ",
        "Safety rules", "safety", "Stay alert")]
    public async Task CreateTipAsync_TrimsAndNormalizesFields(
        string title,
        string category,
        string text,
        string expectedTitle,
        string expectedCategory,
        string expectedText)
    {
        // Arrange
        var dto = new CreateTipDTO
        {
            title = title,
            category = category,
            text = text
        };

        Tip? createdTip = null;

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Tip>()))
            .Callback<Tip>(tip => createdTip = tip)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateTipAsync(dto);

        // Assert
        Assert.Equal(expectedTitle, result.title);
        Assert.Equal(expectedCategory, result.category);
        Assert.Equal(expectedText, result.text);

        Assert.NotNull(createdTip);
        Assert.Equal(expectedTitle, createdTip.title);
        Assert.Equal(expectedCategory, createdTip.category);
        Assert.Equal(expectedText, createdTip.text);

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

        _repositoryMock.Verify(r => r.GetByIdAsync("1"), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(existingTip), Times.Once);
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

        _repositoryMock.Verify(r => r.GetByIdAsync("999"), Times.Once);
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

        _repositoryMock.Verify(r => r.UpdateAsync(existingTip), Times.Once);
    }

    [Theory]
    [InlineData(" ", "travel", "text")]
    [InlineData("title", " ", "text")]
    [InlineData("title", "travel", " ")]
    public async Task UpdateTipAsync_WhenOneFieldIsEmpty_KeepsThatOldValue(
        string title,
        string category,
        string text)
    {
        // Arrange
        var existingTip = new Tip
        {
            id = "1",
            title = "Old title",
            category = "old-category",
            text = "Old text"
        };

        var dto = new UpdateTipDTO
        {
            title = title,
            category = category,
            text = text
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

        if (string.IsNullOrWhiteSpace(title))
            Assert.Equal("Old title", result.title);

        if (string.IsNullOrWhiteSpace(category))
            Assert.Equal("old-category", result.category);

        if (string.IsNullOrWhiteSpace(text))
            Assert.Equal("Old text", result.text);

        _repositoryMock.Verify(r => r.UpdateAsync(existingTip), Times.Once);
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
        _repositoryMock.Verify(r => r.DeleteAsync("999"), Times.Once);
    }
}
