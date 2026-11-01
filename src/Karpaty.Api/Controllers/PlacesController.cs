using Karpaty.Api.Models;
using Karpaty.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Karpaty.Api.Controllers;

/// <summary>
/// Контролер для роботи з місцями.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlacesController : ControllerBase
{
    private readonly IPlaceService _placeService;

    /// <summary>
    /// Ініціалізує контролер місць.
    /// </summary>
    /// <param name="placeService">Сервіс для роботи з місцями.</param>
    public PlacesController(IPlaceService placeService)
    {
        _placeService = placeService;
    }

    /// <summary>
    /// Отримати список усіх місць.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Place>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var places = await _placeService.GetAllPlacesAsync();
        return Ok(places);
    }
}