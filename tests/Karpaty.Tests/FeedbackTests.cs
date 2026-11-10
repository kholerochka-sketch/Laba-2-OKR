using Karpaty.Api.Models;
using Xunit;

namespace Karpaty.Tests
{
    public class FeedbackTests
    {
        [Fact]
        public void Feedback_StoresAllProperties()
        {
            var feedback = new Feedback
            {
                Id = "1",
                Name = "Andrii",
                Email = "test@gmail.com",
                Rating = 5,
                Message = "Все чудово!"
            };

            Assert.Equal("1", feedback.Id);
            Assert.Equal("Andrii", feedback.Name);
            Assert.Equal("test@gmail.com", feedback.Email);
            Assert.Equal(5, feedback.Rating);
            Assert.Equal("Все чудово!", feedback.Message);
        }

        [Fact]
        public void Feedback_HasDefaultStringValues()
        {
            var feedback = new Feedback();

            Assert.Equal(string.Empty, feedback.Name);
            Assert.Equal(string.Empty, feedback.Email);
            Assert.Equal(string.Empty, feedback.Message);
        }

        [Fact]
        public void Feedback_CreatedAtIsSetInUtc()
        {
            var before = DateTime.UtcNow;

            var feedback = new Feedback();

            var after = DateTime.UtcNow;

            Assert.InRange(feedback.CreatedAt, before, after);
            Assert.Equal(DateTimeKind.Utc, feedback.CreatedAt.Kind);
        }
    }
}

