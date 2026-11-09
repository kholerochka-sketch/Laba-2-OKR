using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Karpaty.Api.Controllers;

namespace Karpaty.Tests
{
    public class PlacesControllerTests
    {
        private readonly Mock<IPlaceService> _serviceMock;
        private readonly PlacesController _controller;

        public PlacesControllerTests()
        {
            _serviceMock = new Mock<IPlaceService>();
            _controller = new PlacesController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            var places = new List<Place>
            {
                new Place
                {
                    id = "1",
                    description = "Test place"
                }
            };

            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(places);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Single(returnedPlaces);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList()
        {
            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Empty(returnedPlaces);
        }

        [Fact]
        public async Task GetAll_WithWaterfallCategory_ReturnsOnlyWaterfalls()
        {
            var places = new List<Place>
            {
                new Place
                {
                    id = "1",
                    name = "Водоспад Шипіт",
                    category = PlaceCategory.waterfall
                },
                new Place
                {
                    id = "2",
                    name = "Оглядовий майданчик",
                    category = PlaceCategory.viewpoint
                },
                new Place
                {
                    id = "3",
                    name = "Полонина Боржава",
                    category = PlaceCategory.meadow
                }
            };

            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync((IEnumerable<Place>)places);

            var serviceResult =
                await _serviceMock.Object.GetAllPlacesAsync();

            Assert.Equal(3, serviceResult.Count());

            var result = await _controller.GetAll("waterfall");

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedPlaces =
                Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Single(returnedPlaces);

            Assert.All(returnedPlaces, p =>
                Assert.Equal(PlaceCategory.waterfall, p.category));
        }

        [Fact]
        public async Task GetAll_WithUnknownCategory_ReturnsBadRequest()
        {
            _serviceMock
                .Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            var result = await _controller.GetAll("unknown");

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
