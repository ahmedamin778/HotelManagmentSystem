using HotelBookingSystem.Application.DTOs.Amenities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Features.Amenities.CreateAmenity;

[ApiController]
[Route("api/Amenities")]
public class CreateAmenityController : ControllerBase
{
    private readonly CreateAmenityHandler _handler;

    public CreateAmenityController(CreateAmenityHandler handler)
    {
        _handler = handler;
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateAmenityDto dto)
    {
        var amenity = await _handler.HandleAsync(new CreateAmenityCommand(dto));

        return CreatedAtRoute(
            "GetAmenityById",
            new { id = amenity.Id },
            amenity);
    }
}
