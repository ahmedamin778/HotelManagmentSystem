using HotelBookingSystem.Application.Strategies.RoomSorting;
using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Interfaces.IRepositories;

public interface IRoomRepository : IGenericRepository<Room>
{
    Task<IEnumerable<Room>> GetActiveRoomsAsync();
    Task<bool> ExistsByRoomNumberAsync(string roomNumber);
    Task<bool> RoomTypeExistsAsync(int roomTypeId);
    Task<IEnumerable<Room>> GetAllWithRoomTypeAsync();
    Task<Room?> GetByIdWithRoomTypeAsync(int id);
    Task<IEnumerable<Room>> GetAllWithRoomTypeAndBookingsAsync(int roomTypeId);
    Task<(IEnumerable<Room> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,int pageSize,int? roomTypeId = null,bool? isActive = null,
        IRoomSortingStrategy? sortingStrategy = null);
}
