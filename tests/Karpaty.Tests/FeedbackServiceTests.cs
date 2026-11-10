
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Karpaty.Tests
{
    public class FeedbackServiceTests
    {
        private readonly Mock<IFeedbackRepository> _mockRepository;
        private readonly FeedbackService _service;

        public FeedbackServiceTests()
        {
            _mockRepository = new Mock<IFeedbackRepository>();

            var loggerMock = new Mock<ILogger<FeedbackService>>();

            _service = new FeedbackService(
                _mockRepository.Object,
                loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAllFeedbacksAsync_ReturnsAllFeedbacks()
        {
            var feedbacks = new List<Feedback>
            {
                new Feedback { Id = "1", Name = "User 1", Message = "Message 1" },
                new Feedback { Id = "2", Name = "User 2", Message = "Message 2" }
            };

            _mockRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(feedbacks);

            var result = await _service.GetAllFeedbacksAsync();

            Assert.Equal(2, result.Count());
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetFeedbackByIdAsync_ReturnsFeedback()
        {
            var feedback = new Feedback
            {
                Id = "1",
                Name = "Test User",
                Message = "Test message"
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(feedback);

            var result = await _service.GetFeedbackByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal("Test User", result.Name);
            _mockRepository.Verify(r => r.GetByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetFeedbackByIdAsync_WhenNotFound_ReturnsNull()
        {
            _mockRepository
                .Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Feedback?)null);

            var result = await _service.GetFeedbackByIdAsync("999");

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateFeedbackAsync_TrimsAndNormalizesFields()
        {
            var dto = new CreateFeedbackDTO
            {
                Name = "  Test User  ",
                Email = "  TEST@MAIL.COM  ",
                Rating = 5,
                Message = "Great place"
            };

            Feedback? createdFeedback = null;

            _mockRepository
                .Setup(r => r.CreateAsync(It.IsAny<Feedback>()))
                .Callback<Feedback>(feedback => createdFeedback = feedback)
                .Returns(Task.CompletedTask);

            var result = await _service.CreateFeedbackAsync(dto);

            Assert.Equal("Test User", result.Name);
            Assert.Equal("test@mail.com", result.Email);
            Assert.Equal(5, result.Rating);

            Assert.NotNull(createdFeedback);
            Assert.Equal("Test User", createdFeedback.Name);

            _mockRepository.Verify(
                r => r.CreateAsync(It.IsAny<Feedback>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_WhenFeedbackExists_UpdatesFields()
        {
            var existing = new Feedback
            {
                Id = "1",
                Name = "Test User",
                Rating = 3,
                Message = "Old message"
            };

            var dto = new UpdateFeedbackDTO
            {
                Rating = 5,
                Message = "New message"
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(existing);

            _mockRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Feedback>()))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateFeedbackAsync("1", dto);

            Assert.NotNull(result);
            Assert.Equal(5, result.Rating);
            Assert.Equal("New message", result.Message);

            _mockRepository.Verify(
                r => r.UpdateAsync(existing),
                Times.Once);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_WhenNotFound_ReturnsNull()
        {
            _mockRepository
                .Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Feedback?)null);

            var dto = new UpdateFeedbackDTO
            {
                Rating = 4,
                Message = "New message"
            };

            var result = await _service.UpdateFeedbackAsync("999", dto);

            Assert.Null(result);

            _mockRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Feedback>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteFeedbackAsync_WhenExists_ReturnsTrue()
        {
            _mockRepository
                .Setup(r => r.DeleteAsync("1"))
                .ReturnsAsync(true);

            var result = await _service.DeleteFeedbackAsync("1");

            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteAsync("1"), Times.Once);
        }

        [Fact]
        public async Task DeleteFeedbackAsync_WhenNotFound_ReturnsFalse()
        {
            _mockRepository
                .Setup(r => r.DeleteAsync("999"))
                .ReturnsAsync(false);

            var result = await _service.DeleteFeedbackAsync("999");

            Assert.False(result);
        }
    }
}
