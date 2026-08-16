using AutoMapper;
using HotelBookingSystem.Application.DTOs.Bookings;
using HotelBookingSystem.Application.DTOs.Reports;
using HotelBookingSystem.Application.DTOs.Rooms;
using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Application.Interfaces.IServices;
using HotelBookingSystem.Core.Enums;

namespace HotelBookingSystem.Application.Services;

public class ReportService : IReportService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public ReportService(IRoomRepository roomRepository,IBookingRepository bookingRepository,
        IReviewRepository reviewRepository,IMapper mapper)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _reviewRepository=reviewRepository;
        _mapper= mapper;
    }

    public async Task<IEnumerable<RoomTypeReviewReportDto>> GetBestReviewedRoomTypesAsync()
    {
        var reviews = await _reviewRepository.GetAllWithDetailsAsync();

        return reviews
            .GroupBy(x => x.Booking.Room.RoomTypeId)
            .Select(group => new RoomTypeReviewReportDto
            {
                RoomTypeName = group.First().Booking.Room.RoomType.Name,

                AverageRating = Math.Round(group.Average(x => (decimal)x.Rating), 2),

                ReviewCount = group.Count()
            })
            .OrderByDescending(x => x.AverageRating)
            .ToList();
    }
    public async Task<DashboardReportDto> GetDashboardAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        var bookings = await _bookingRepository.GetAllAsync();

        return new DashboardReportDto
        {
            TotalRooms = rooms.Count(),

            ActiveRooms = rooms.Count(x => x.IsActive),

            InactiveRooms = rooms.Count(x =>x.IsActive==false),

            TotalBookings = bookings.Count(),

            RequestedBookings = bookings.Count(
                x => x.Status == BookingStatus.Requested),

            ConfirmedBookings = bookings.Count(
                x => x.Status == BookingStatus.Confirmed),

            CheckedInBookings = bookings.Count(
                x => x.Status == BookingStatus.CheckedIn),

            CheckedOutBookings = bookings.Count(
                x => x.Status == BookingStatus.CheckedOut),

            CancelledBookings = bookings.Count(
                x => x.Status == BookingStatus.Cancelled),

            TotalRevenue = bookings
                .Where(x => x.Status != BookingStatus.Cancelled)
                .Sum(x => x.TotalPrice)
        };
    }

    public async Task<OccupancyReportDto> GetOccupancyAsync()
    {
        var rooms = await _roomRepository.GetAllAsync();
        var bookings = await _bookingRepository.GetAllAsync();

        var totalActiveRooms = rooms.Count(x => x.IsActive);

        var occupiedRooms = bookings
            .Where(x => x.Status == BookingStatus.CheckedIn)
            .Select(x => x.RoomId)
            .Distinct()
            .Count();
        
        var occupancyPercentage = totalActiveRooms == 0
            ? 0
            : (decimal)occupiedRooms / totalActiveRooms * 100;

        return new OccupancyReportDto
        {
            TotalActiveRooms = totalActiveRooms,
            OccupiedRooms = occupiedRooms,
            OccupancyPercentage = occupancyPercentage
        };
    }

    public async Task<IEnumerable<BookingDto>> GetGuestBookingHistoryAsync(string guestId)
    {
        var bookings = await _bookingRepository
            .GetByGuestIdAsync(guestId);

        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }


}