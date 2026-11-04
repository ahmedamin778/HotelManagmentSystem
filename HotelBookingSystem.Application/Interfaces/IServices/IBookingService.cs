using HotelBookingSystem.Application.DTOs.Bookings;
using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.DTOs.Rooms;

namespace HotelBookingSystem.Application.Interfaces.IServices;

public interface IBookingService
{
    

    Task<BookingDto> CreateAsync(string guestId, Guid idempotencyKey, CreateBookingDto dto);

    Task<IEnumerable<BookingDto>> GetMyBookingsAsync(string guestId);

    Task<BookingDto?> GetMyBookingByIdAsync(string guestId,int bookingId);

    Task<bool> CancelMyBookingAsync(string guestId, int bookingId);

    Task<PaginatedResultDto<BookingDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<BookingDto?> GetByIdAsync(int bookingId);

    Task<bool> ConfirmAsync(int bookingId);

    Task<bool> CancelAsync(int bookingId);

    Task<bool> CheckInAsync(int ibookingIdd);

    Task<bool> CheckOutAsync(int bookingId);

    Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateTime checkInDate,DateTime checkOutDate);
}
