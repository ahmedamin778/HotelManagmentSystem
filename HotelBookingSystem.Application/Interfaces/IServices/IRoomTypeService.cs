using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.DTOs.RoomTypes;

namespace HotelBookingSystem.Application.Interfaces.IServices;

public interface IRoomTypeService
{
    Task<PaginatedResultDto<RoomTypeDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<RoomTypeDto?> GetByIdAsync(int id);

    Task<IEnumerable<RoomTypeDto>> GetByAmenityIdAsync(int amenityId);

    Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto);

    Task<bool> UpdateAsync(int id, UpdateRoomTypeDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> AddAmenityAsync(int roomTypeId, int amenityId);

    Task<bool> RemoveAmenityAsync(int roomTypeId, int amenityId);
}