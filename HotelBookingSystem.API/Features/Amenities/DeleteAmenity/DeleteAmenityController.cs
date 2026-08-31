using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Features.Amenities.DeleteAmenity;

[ApiController]
[Route("api/Amenities")]
public class DeleteAmenityController : ControllerBase
{
    private readonly DeleteAmenityHandler _handler;

    public DeleteAmenityController(DeleteAmenityHandler handler)
    {
        _handler = handler;
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _handler.HandleAsync(new DeleteAmenityCommand(id));

        return Ok("Amenity deleted successfully.");
    }
}
