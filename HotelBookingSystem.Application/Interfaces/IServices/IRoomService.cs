using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.DTOs.Rooms;

namespace HotelBookingSystem.Application.Interfaces.IServices;

public interface IRoomService
{
    Task<PaginatedResultDto<RoomDto>> GetAllAsync(int pageNumber,int pageSize,int? roomTypeId = null,
        bool? isActive = null,string sortBy = "roomNumber");
    Task<RoomDto?> GetByIdAsync(int id);

    Task<RoomDto> CreateAsync(CreateRoomDto dto);

    Task<bool> UpdateAsync(int id, UpdateRoomDto dto);

    Task<bool> DeactivateAsync(int id);

    Task<bool> ActivateAsync(int id);
    Task<IEnumerable<RoomDto>> GetActiveRoomsAsync();
}
