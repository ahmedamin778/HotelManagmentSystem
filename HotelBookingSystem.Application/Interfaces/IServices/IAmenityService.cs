using HotelBookingSystem.Application.DTOs.Amenities;
using HotelBookingSystem.Application.DTOs.Common;

namespace HotelBookingSystem.Application.Interfaces.IServices;

public interface IAmenityService
{
    Task<PaginatedResultDto<AmenityDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<AmenityDto?> GetByIdAsync(int id);

    Task<AmenityDto> CreateAsync(CreateAmenityDto dto);

    Task<bool> UpdateAsync(int id,UpdateAmenityDto dto);

    Task<bool> DeleteAsync(int id);
}