using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Features.Amenities.GetAmenities;

[ApiController]
[Route("api/Amenities")]
public class GetAmenitiesController : ControllerBase
{
    private readonly GetAmenitiesHandler _handler;

    public GetAmenitiesController(GetAmenitiesHandler handler)
    {
        _handler = handler;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var amenities = await _handler.HandleAsync(
            new GetAmenitiesQuery(pageNumber, pageSize));

        return Ok(amenities);
    }
}
