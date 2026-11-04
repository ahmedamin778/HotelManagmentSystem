using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Interfaces.IRepositories;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate);
    Task<IEnumerable<Booking>> GetByGuestIdAsync(string guestId);

    Task<Booking?> GetByIdWithDetailsAsync(int id);

    Task<Booking?> GetByIdempotencyKeyAsync(string guestId, Guid idempotencyKey);

    Task<IEnumerable<Booking>> GetAllWithDetailsAsync();

    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate);
    Task<(IEnumerable<Booking> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}
