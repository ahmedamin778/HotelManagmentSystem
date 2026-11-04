using HotelBookingSystem.Application.DTOs.Amenities;

namespace HotelBookingSystem.Api.Features.Amenities.GetAmenities;

public class AmenitiesV2Response
{
    public string ApiVersion { get; set; } = "2.0";

    public IEnumerable<AmenityDto> Amenities { get; set; } = [];

    public AmenitiesPaginationV2 Pagination { get; set; } = new();
}

public class AmenitiesPaginationV2
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}
