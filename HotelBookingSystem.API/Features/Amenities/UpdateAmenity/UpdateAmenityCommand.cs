using HotelBookingSystem.Application.DTOs.Amenities;

namespace HotelBookingSystem.Api.Features.Amenities.UpdateAmenity;

public record UpdateAmenityCommand(int Id, UpdateAmenityDto Dto);
