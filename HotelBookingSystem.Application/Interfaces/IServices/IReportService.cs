using HotelBookingSystem.Application.DTOs.Bookings;
using HotelBookingSystem.Application.DTOs.Reports;
using HotelBookingSystem.Application.DTOs.Rooms;

namespace HotelBookingSystem.Application.Interfaces.IServices;

public interface IReportService
{
    Task<DashboardReportDto> GetDashboardAsync();
    Task<OccupancyReportDto> GetOccupancyAsync();
    Task<IEnumerable<RoomTypeReviewReportDto>>GetBestReviewedRoomTypesAsync();
    Task<IEnumerable<BookingDto>> GetGuestBookingHistoryAsync(string guestId);

}