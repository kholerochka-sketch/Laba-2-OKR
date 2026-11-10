
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Karpaty.Api.Models;
using Karpaty.Api.DTO;
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
                new Place { id = "1", description = "Test place" }
            };

            _serviceMock.Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(places);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaces = Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Single(returnedPlaces);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList()
        {
            _serviceMock.Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaces = Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Empty(returnedPlaces);
        }

        [Fact]
        public async Task GetAll_WithWaterfallCategory_ReturnsOnlyWaterfalls()
        {
            var places = new List<Place>
            {
                new Place { id = "1", name = "Водоспад", category = PlaceCategory.waterfall },
                new Place { id = "2", name = "Оглядовий майданчик", category = PlaceCategory.viewpoint },
                new Place { id = "3", name = "Полонина", category = PlaceCategory.meadow }
            };

            _serviceMock.Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync((IEnumerable<Place>)places);

            var result = await _controller.GetAll("waterfall");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaces = Assert.IsAssignableFrom<IEnumerable<Place>>(okResult.Value);

            Assert.Single(returnedPlaces);
            Assert.All(returnedPlaces, p =>
                Assert.Equal(PlaceCategory.waterfall, p.category));
        }

        [Fact]
        public async Task GetAll_WithUnknownCategory_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            var result = await _controller.GetAll("unknown");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // GET BY ID

        [Fact]
        public async Task GetById_WhenPlaceExists_ReturnsOk()
        {
            var place = new Place { id = "1", name = "Водоспад" };

            _serviceMock.Setup(s => s.GetPlaceByIdAsync("1"))
                .ReturnsAsync(place);

            var result = await _controller.GetById("1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(place, okResult.Value);
        }

        [Fact]
        public async Task GetById_WhenPlaceDoesNotExist_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetPlaceByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            var result = await _controller.GetById("999");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_WhenIdFormatIsInvalid_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetPlaceByIdAsync("invalid"))
                .ThrowsAsync(new System.FormatException());

            var result = await _controller.GetById("invalid");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // CREATE

        [Fact]
        public async Task Create_WhenPlaceIsNew_ReturnsCreatedAtAction()
        {
            var dto = new CreatePlaceDTO { name = "Нове місце" };
            var createdPlace = new Place { id = "1", name = dto.name };

            _serviceMock.Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>());

            _serviceMock.Setup(s => s.CreatePlaceAsync(dto))
                .ReturnsAsync(createdPlace);

            var result = await _controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(nameof(PlacesController.GetById), createdResult.ActionName);
            Assert.Equal(createdPlace, createdResult.Value);
        }

        [Fact]
        public async Task Create_WhenNameAlreadyExists_ReturnsBadRequest()
        {
            var dto = new CreatePlaceDTO { name = "Водоспад" };

            _serviceMock.Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>
                {
                    new Place { id = "1", name = "Водоспад" }
                });

            var result = await _controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.Verify(s => s.CreatePlaceAsync(It.IsAny<CreatePlaceDTO>()), Times.Never);
        }

        // UPDATE

        [Fact]
        public async Task Update_WhenPlaceExists_ReturnsNoContent()
        {
            var existingPlace = new Place { id = "1", name = "Водоспад" };
            var dto = new UpdatePlaceDTO { description = "Новий опис" };

            _serviceMock.Setup(s => s.GetPlaceByIdAsync("1"))
                .ReturnsAsync(existingPlace);

            _serviceMock.Setup(s => s.UpdatePlaceAsync("1", dto))
                .ReturnsAsync(existingPlace);

            var result = await _controller.Update("1", dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_WhenPlaceDoesNotExist_ReturnsNotFound()
        {
            var dto = new UpdatePlaceDTO { description = "Новий опис" };

            _serviceMock.Setup(s => s.GetPlaceByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            var result = await _controller.Update("999", dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_WhenNoFieldsProvided_ReturnsBadRequest()
        {
            var dto = new UpdatePlaceDTO();

            var result = await _controller.Update("1", dto);

            Assert.IsType<BadRequestObjectResult>(result);

            _serviceMock.Verify(s => s.GetPlaceByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Update_WhenNameAlreadyExists_ReturnsBadRequest()
        {
            var existingPlace = new Place { id = "1", name = "Старе місце" };
            var dto = new UpdatePlaceDTO { name = "Інше місце" };

            _serviceMock.Setup(s => s.GetPlaceByIdAsync("1"))
                .ReturnsAsync(existingPlace);

            _serviceMock.Setup(s => s.GetAllPlacesAsync())
                .ReturnsAsync(new List<Place>
                {
                    existingPlace,
                    new Place { id = "2", name = "Інше місце" }
                });

            var result = await _controller.Update("1", dto);

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.Verify(s => s.UpdatePlaceAsync(
                It.IsAny<string>(), It.IsAny<UpdatePlaceDTO>()), Times.Never);
        }

        // DELETE

        [Fact]
        public async Task Delete_WhenPlaceExists_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.GetPlaceByIdAsync("1"))
                .ReturnsAsync(new Place { id = "1", name = "Водоспад" });

            _serviceMock.Setup(s => s.DeletePlaceAsync("1"))
                .ReturnsAsync(true);

            var result = await _controller.Delete("1");

            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.DeletePlaceAsync("1"), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenPlaceDoesNotExist_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetPlaceByIdAsync("999"))
                .ReturnsAsync((Place?)null);

            var result = await _controller.Delete("999");

            Assert.IsType<NotFoundObjectResult>(result);
            _serviceMock.Verify(s => s.DeletePlaceAsync("999"), Times.Never);
        }
    }
}
