using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingSystem.Api.Features.Amenities.GetAmenities;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/amenities")]
public class GetAmenitiesV2Controller : ControllerBase
{
    private readonly GetAmenitiesHandler _handler;

    public GetAmenitiesV2Controller(GetAmenitiesHandler handler)
    {
        _handler = handler;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _handler.HandleAsync(
            new GetAmenitiesQuery(pageNumber, pageSize));

        return Ok(new AmenitiesV2Response
        {
            ApiVersion = "2.0",
            Amenities = result.Items,
            Pagination = new AmenitiesPaginationV2
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            }
        });
    }
}
