
using Karpaty.Api.DBCommunication.Repositories.Interfaces;
using Karpaty.Api.DTO;
using Karpaty.Api.Models;
using Karpaty.Api.Services;
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
            _service = new FeedbackService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllFeedbacksAsync_ReturnsAllFeedbacks()
        {
            var feedbacks = new List<Feedback>
            {
                new Feedback { Id = "1", Name = "Andriy", Rating = 5 },
                new Feedback { Id = "2", Name = "Oleh", Rating = 4 }
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(feedbacks);

            var result = await _service.GetAllFeedbacksAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetFeedbackByIdAsync_WhenExists_ReturnsFeedback()
        {
            var feedback = new Feedback
            {
                Id = "1",
                Name = "Andriy",
                Email = "test@example.com",
                Rating = 5,
                Message = "Great!"
            };

            _mockRepository.Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(feedback);

            var result = await _service.GetFeedbackByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal("Andriy", result.Name);
        }

        [Fact]
        public async Task GetFeedbackByIdAsync_WhenNotExists_ReturnsNull()
        {
            _mockRepository.Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Feedback?)null);

            var result = await _service.GetFeedbackByIdAsync("999");

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateFeedbackAsync_TrimsNameAndNormalizesEmail()
        {
            var dto = new CreateFeedbackDTO
            {
                Name = "  Andriy  ",
                Email = "  TEST@EXAMPLE.COM  ",
                Rating = 5,
                Message = "Great service!"
            };

            var result = await _service.CreateFeedbackAsync(dto);

            Assert.Equal("Andriy", result.Name);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal(5, result.Rating);
            Assert.Equal("Great service!", result.Message);

            _mockRepository.Verify(
                r => r.CreateAsync(It.Is<Feedback>(f =>
                    f.Name == "Andriy" &&
                    f.Email == "test@example.com" &&
                    f.Rating == 5)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_WhenExists_UpdatesFeedback()
        {
            var existing = new Feedback
            {
                Id = "1",
                Name = "Andriy",
                Email = "test@example.com",
                Rating = 3,
                Message = "Old message"
            };

            var dto = new UpdateFeedbackDTO
            {
                Rating = 5,
                Message = "  GREAT SERVICE!  "
            };

            _mockRepository.Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(existing);

            var result = await _service.UpdateFeedbackAsync("1", dto);

            Assert.NotNull(result);
            Assert.Equal(5, result.Rating);
            Assert.Equal("great service!", result.Message);

            _mockRepository.Verify(
                r => r.UpdateAsync(existing),
                Times.Once);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_WhenNotExists_ReturnsNull()
        {
            var dto = new UpdateFeedbackDTO
            {
                Rating = 5,
                Message = "New message"
            };

            _mockRepository.Setup(r => r.GetByIdAsync("999"))
                .ReturnsAsync((Feedback?)null);

            var result = await _service.UpdateFeedbackAsync("999", dto);

            Assert.Null(result);

            _mockRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Feedback>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_WithEmptyMessage_PreservesOldMessage()
        {
            var existing = new Feedback
            {
                Id = "1",
                Rating = 4,
                Message = "Old message"
            };

            var dto = new UpdateFeedbackDTO
            {
                Message = "   "
            };

            _mockRepository.Setup(r => r.GetByIdAsync("1"))
                .ReturnsAsync(existing);

            var result = await _service.UpdateFeedbackAsync("1", dto);

            Assert.NotNull(result);
            Assert.Equal("Old message", result.Message);
            Assert.Equal(0, result.Rating);
        }

        [Fact]
        public async Task DeleteFeedbackAsync_WhenExists_ReturnsTrue()
        {
            _mockRepository.Setup(r => r.DeleteAsync("1"))
                .ReturnsAsync(true);

            var result = await _service.DeleteFeedbackAsync("1");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteFeedbackAsync_WhenNotExists_ReturnsFalse()
        {
            _mockRepository.Setup(r => r.DeleteAsync("999"))
                .ReturnsAsync(false);

            var result = await _service.DeleteFeedbackAsync("999");

            Assert.False(result);
        }
    }
}
