using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Features.Amenities.GetAmenityById;

[ApiController]
[Route("api/Amenities")]
public class GetAmenityByIdController : ControllerBase
{
    private readonly GetAmenityByIdHandler _handler;

    public GetAmenityByIdController(GetAmenityByIdHandler handler)
    {
        _handler = handler;
    }

    [AllowAnonymous]
    [HttpGet("{id:int}", Name = "GetAmenityById")]
    public async Task<IActionResult> GetById(int id)
    {
        var amenity = await _handler.HandleAsync(new GetAmenityByIdQuery(id));

        if (amenity is null)
            return NotFound("Amenity not found.");

        return Ok(amenity);
    }
}
