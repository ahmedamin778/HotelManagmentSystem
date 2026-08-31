using HotelBookingSystem.Application.DTOs.Amenities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Features.Amenities.UpdateAmenity;

[ApiController]
[Route("api/Amenities")]
public class UpdateAmenityController : ControllerBase
{
    private readonly UpdateAmenityHandler _handler;

    public UpdateAmenityController(UpdateAmenityHandler handler)
    {
        _handler = handler;
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAmenityDto dto)
    {
        await _handler.HandleAsync(new UpdateAmenityCommand(id, dto));

        return Ok("Amenity updated successfully.");
    }
}
