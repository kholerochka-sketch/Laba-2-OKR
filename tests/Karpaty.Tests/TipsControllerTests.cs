
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karpaty.Api.Controllers;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Karpaty.Tests
{
    public class TipsControllerTests
    {
        private readonly Mock<ITipService> _serviceMock;
        private readonly TipsController _controller;

        public TipsControllerTests()
        {
            // Arrange
            _serviceMock = new Mock<ITipService>();
            _controller = new TipsController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithAllTips()
        {
            // Arrange
            var tips = new List<Tip>
            {
                new Tip { id = "1", title = "Порада 1", category = "safety", text = "Текст 1" },
                new Tip { id = "2", title = "Порада 2", category = "gear", text = "Текст 2" }
            };

            _serviceMock
                .Setup(s => s.GetAllTipsAsync())
                .ReturnsAsync(tips);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTips =
                Assert.IsAssignableFrom<IEnumerable<Tip>>(okResult.Value);

            Assert.Equal(2, returnedTips.Count());
            _serviceMock.Verify(s => s.GetAllTipsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenNoTipsExist_ReturnsEmptyList()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetAllTipsAsync())
                .ReturnsAsync(new List<Tip>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTips =
                Assert.IsAssignableFrom<IEnumerable<Tip>>(okResult.Value);

            Assert.Empty(returnedTips);
            _serviceMock.Verify(s => s.GetAllTipsAsync(), Times.Once);
        }

        [Theory]
        [InlineData("SAFETY", "safety")]
        [InlineData("GEAR", "gear")]
        public async Task GetAll_WithCategory_ReturnsFilteredTips(
            string category,
            string expectedCategory)
        {
            // Arrange
            var tips = new List<Tip>
            {
                new Tip { id = "1", title = "Порада 1", category = "safety", text = "Текст 1" },
                new Tip { id = "2", title = "Порада 2", category = "gear", text = "Текст 2" }
            };

            _serviceMock
                .Setup(s => s.GetAllTipsAsync())
                .ReturnsAsync(tips);

            // Act
            var result = await _controller.GetAll(category);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTips =
                Assert.IsAssignableFrom<IEnumerable<Tip>>(okResult.Value);

            Assert.Single(returnedTips);
            Assert.Equal(expectedCategory, returnedTips.First().category);

            _serviceMock.Verify(s => s.GetAllTipsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenTipExists_ReturnsOk()
        {
            // Arrange
            var tip = new Tip
            {
                id = "1",
                title = "Похід",
                category = "safety",
                text = "Візьми аптечку"
            };

            _serviceMock
                .Setup(s => s.GetTipByIdAsync("1"))
                .ReturnsAsync(tip);

            // Act
            var result = await _controller.GetById("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(tip, okResult.Value);

            _serviceMock.Verify(s => s.GetTipByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenTipDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetTipByIdAsync("999"))
                .ReturnsAsync((Tip?)null);

            // Act
            var result = await _controller.GetById("999");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _serviceMock.Verify(s => s.GetTipByIdAsync("999"), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateTipDTO
            {
                title = "Нова порада",
                category = "safety",
                text = "Візьми воду"
            };

            var createdTip = new Tip
            {
                id = "1",
                title = dto.title,
                category = dto.category,
                text = dto.text
            };

            _serviceMock
                .Setup(s => s.CreateTipAsync(dto))
                .ReturnsAsync(createdTip);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(nameof(TipsController.GetById), createdResult.ActionName);
            Assert.Equal(createdTip, createdResult.Value);

            _serviceMock.Verify(s => s.CreateTipAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Update_WhenTipExists_ReturnsNoContent()
        {
            // Arrange
            var dto = new UpdateTipDTO
            {
                title = "Оновлена порада"
            };

            _serviceMock
                .Setup(s => s.UpdateTipAsync("1", dto))
                .ReturnsAsync(new Tip
                {
                    id = "1",
                    title = "Оновлена порада"
                });

            // Act
            var result = await _controller.Update("1", dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.UpdateTipAsync("1", dto), Times.Once);
        }

        [Fact]
        public async Task Update_WhenTipDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var dto = new UpdateTipDTO
            {
                title = "Оновлена порада"
            };

            _serviceMock
                .Setup(s => s.UpdateTipAsync("999", dto))
                .ReturnsAsync((Tip?)null);

            // Act
            var result = await _controller.Update("999", dto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _serviceMock.Verify(s => s.UpdateTipAsync("999", dto), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenTipExists_ReturnsNoContent()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteTipAsync("1"))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.DeleteTipAsync("1"), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenTipDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteTipAsync("999"))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete("999");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _serviceMock.Verify(s => s.DeleteTipAsync("999"), Times.Once);
        }
    }
}
