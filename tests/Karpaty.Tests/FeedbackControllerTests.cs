
using System;
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
    public class FeedbackControllerTests
    {
        private readonly Mock<IFeedbackService> _serviceMock;
        private readonly FeedbackController _controller;

        public FeedbackControllerTests()
        {
            _serviceMock = new Mock<IFeedbackService>();
            _controller = new FeedbackController(_serviceMock.Object);
        }

        // CREATE

        [Fact]
        public async Task Create_ReturnsCreatedFeedback()
        {
            var dto = new CreateFeedbackDTO
            {
                Name = "Олена",
                Email = "olena@example.com",
                Rating = 5,
                Message = "Дуже сподобався маршрут!"
            };

            var feedback = new Feedback
            {
                Id = "1",
                Name = dto.Name,
                Email = dto.Email,
                Rating = dto.Rating,
                Message = dto.Message
            };

            _serviceMock.Setup(s => s.CreateFeedbackAsync(dto))
                .ReturnsAsync(feedback);

            var result = await _controller.Create(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, objectResult.StatusCode);
            Assert.Equal(feedback, objectResult.Value);
        }

        // GET ALL

        [Fact]
        public async Task GetAll_ReturnsAllFeedbacks()
        {
            var feedbacks = new List<Feedback>
            {
                new Feedback { Id = "1", Name = "Олена" },
                new Feedback { Id = "2", Name = "Андрій" }
            };

            _serviceMock.Setup(s => s.GetAllFeedbacksAsync())
                .ReturnsAsync(feedbacks);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedFeedbacks =
                Assert.IsAssignableFrom<IEnumerable<Feedback>>(okResult.Value);

            Assert.Equal(2, returnedFeedbacks.Count());
        }

        [Fact]
        public async Task GetAll_ReturnsFeedbacksNewestFirst()
        {
            var oldFeedback = new Feedback
            {
                Id = "1",
                Name = "Олена",
                CreatedAt = new DateTime(2026, 1, 1)
            };

            var newFeedback = new Feedback
            {
                Id = "2",
                Name = "Андрій",
                CreatedAt = new DateTime(2026, 2, 1)
            };

            _serviceMock.Setup(s => s.GetAllFeedbacksAsync())
                .ReturnsAsync(new List<Feedback>
                {
                    oldFeedback,
                    newFeedback
                });

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedFeedbacks =
                Assert.IsAssignableFrom<IEnumerable<Feedback>>(okResult.Value)
                    .ToList();

            Assert.Equal("2", returnedFeedbacks[0].Id);
            Assert.Equal("1", returnedFeedbacks[1].Id);
        }

        // GET BY ID

        [Fact]
        public async Task GetById_WhenFeedbackExists_ReturnsOk()
        {
            var feedback = new Feedback
            {
                Id = "1",
                Name = "Олена",
                Rating = 5,
                Message = "Чудова подорож!"
            };

            _serviceMock.Setup(s => s.GetFeedbackByIdAsync("1"))
                .ReturnsAsync(feedback);

            var result = await _controller.GetById("1");

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(feedback, okResult.Value);
        }

        [Fact]
        public async Task GetById_WhenFeedbackDoesNotExist_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetFeedbackByIdAsync("999"))
                .ReturnsAsync((Feedback?)null);

            var result = await _controller.GetById("999");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // UPDATE

        [Fact]
        public async Task Update_WhenFeedbackExists_ReturnsNoContent()
        {
            var dto = new UpdateFeedbackDTO
            {
                Rating = 4,
                Message = "Оновлений текст відгуку!"
            };

            _serviceMock.Setup(s => s.UpdateFeedbackAsync("1", dto))
                .ReturnsAsync(new Feedback
                {
                    Id = "1",
                    Rating = 4,
                    Message = "Оновлений текст відгуку!"
                });

            var result = await _controller.Update("1", dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_WhenFeedbackDoesNotExist_ReturnsNotFound()
        {
            var dto = new UpdateFeedbackDTO
            {
                Rating = 4
            };

            _serviceMock.Setup(s => s.UpdateFeedbackAsync("999", dto))
                .ReturnsAsync((Feedback?)null);

            var result = await _controller.Update("999", dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_WhenNoFieldsProvided_ReturnsBadRequest()
        {
            var dto = new UpdateFeedbackDTO();

            var result = await _controller.Update("1", dto);

            Assert.IsType<BadRequestObjectResult>(result);

            _serviceMock.Verify(
                s => s.UpdateFeedbackAsync(
                    It.IsAny<string>(),
                    It.IsAny<UpdateFeedbackDTO>()),
                Times.Never);
        }

        // DELETE

        [Fact]
        public async Task Delete_WhenFeedbackExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteFeedbackAsync("1"))
                .ReturnsAsync(true);

            var result = await _controller.Delete("1");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_WhenFeedbackDoesNotExist_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteFeedbackAsync("999"))
                .ReturnsAsync(false);

            var result = await _controller.Delete("999");

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
