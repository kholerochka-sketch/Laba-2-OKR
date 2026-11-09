using Karpaty.Api.Models;
using Xunit;

namespace Karpaty.Tests
{
    public class PlaceTests
    {
        [Fact]
        public void Place_StoresAllProperties()
        {
            var place = new Place
            {
                id = "1",
                name = "Водоспад",
                category = PlaceCategory.waterfall,
                region = PlaceRegion.Західні_Карпати,
                description = "Гарне місце",
                difficulty = PlaceDifficulty.easy,
                latitude = 48.123f,
                longitude = 24.456f,
                imageUrl = "image.jpg"
            };

            Assert.Equal("1", place.id);
            Assert.Equal("Водоспад", place.name);
            Assert.Equal(PlaceCategory.waterfall, place.category);
            Assert.Equal(PlaceRegion.Західні_Карпати, place.region);
            Assert.Equal("Гарне місце", place.description);
            Assert.Equal(PlaceDifficulty.easy, place.difficulty);
            Assert.Equal(48.123f, place.latitude);
            Assert.Equal(24.456f, place.longitude);
            Assert.Equal("image.jpg", place.imageUrl);
        }

        [Fact]
        public void Place_AcceptsValidCategory()
        {
            var place = new Place
            {
                category = PlaceCategory.viewpoint
            };

            Assert.Equal(PlaceCategory.viewpoint, place.category);
        }

        [Fact]
        public void Place_AcceptsValidDifficulty()
        {
            var place = new Place
            {
                difficulty = PlaceDifficulty.hard
            };

            Assert.Equal(PlaceDifficulty.hard, place.difficulty);
        }
    }
}
