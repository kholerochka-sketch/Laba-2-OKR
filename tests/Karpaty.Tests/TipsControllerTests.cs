
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
        private readonly Mock<ITipService> _mockService;
        private readonly TipsController _controller;

        public TipsControllerTests()
        {
            _mockService = new Mock<ITipService>();
            _controller = new TipsController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithAllTips()
        {
            var tips = new List<Tip>
            {
                new Tip { id = "1", title = "Порада 1", category = "safety", text = "Текст 1" },
                new Tip { id = "2", title = "Порада 2", category = "gear", text = "Текст 2" }
            };

            _mockService.Setup(s => s.GetAllTipsAsync())
                .ReturnsAsync(tips);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTips = Assert.IsAssignableFrom<IEnumerable<Tip>>(okResult.Value);

            Assert.Equal(2, returnedTips.Count());
        }

        [Fact]
        public async Task GetAll_WithCategory_ReturnsFilteredTips()
        {
            var tips = new List<Tip>
            {
                new Tip { id = "1", title = "Порада 1", category = "safety", text = "Текст 1" },
                new Tip { id = "2", title = "Порада 2", category = "gear", text = "Текст 2" }
            };

            _mockService.Setup(s => s.GetAllTipsAsync())
                .ReturnsAsync(tips);

            var result = await _controller.GetAll("SAFETY");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTips = Assert.IsAssignableFrom<IEnumerable<Tip>>(okResult.Value);

            Assert.Single(returnedTips);
            Assert.Equal("safety", returnedTips.First().category);
        }

        [Fact]
        public async Task GetById_WhenTipExists_ReturnsOk()
        {
            var tip = new Tip
            {
                id = "1",
                title = "Похід",
                category = "safety",
                text = "Візьми аптечку"
            };

            _mockService.Setup(s => s.GetTipByIdAsync("1"))
                .ReturnsAsync(tip);

            var result = await _controller.GetById("1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(tip, okResult.Value);
        }

        [Fact]
        public async Task GetById_WhenTipDoesNotExist_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetTipByIdAsync("999"))
                .ReturnsAsync((Tip?)null);

            var result = await _controller.GetById("999");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
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

            _mockService.Setup(s => s.CreateTipAsync(dto))
                .ReturnsAsync(createdTip);

            var result = await _controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(nameof(TipsController.GetById), createdResult.ActionName);
            Assert.Equal(createdTip, createdResult.Value);
        }

        [Fact]
        public async Task Update_WhenTipExists_ReturnsNoContent()
        {
            var dto = new UpdateTipDTO { title = "Оновлена порада" };

            _mockService.Setup(s => s.UpdateTipAsync("1", dto))
                .ReturnsAsync(new Tip { id = "1", title = "Оновлена порада" });

            var result = await _controller.Update("1", dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_WhenTipDoesNotExist_ReturnsNotFound()
        {
            var dto = new UpdateTipDTO { title = "Оновлена порада" };

            _mockService.Setup(s => s.UpdateTipAsync("999", dto))
                .ReturnsAsync((Tip?)null);

            var result = await _controller.Update("999", dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_WhenTipExists_ReturnsNoContent()
        {
            _mockService.Setup(s => s.DeleteTipAsync("1"))
                .ReturnsAsync(true);

            var result = await _controller.Delete("1");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_WhenTipDoesNotExist_ReturnsNotFound()
        {
            _mockService.Setup(s => s.DeleteTipAsync("999"))
                .ReturnsAsync(false);

            var result = await _controller.Delete("999");

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
