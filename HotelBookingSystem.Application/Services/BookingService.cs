using AutoMapper;
using HotelBookingSystem.Application.DTOs.Bookings;
using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.DTOs.Rooms;
using HotelBookingSystem.Application.Exceptions;
using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Application.Interfaces.IServices;
using HotelBookingSystem.Core.Enums;
using HotelBookingSystem.Core.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Logging;

namespace HotelBookingSystem.Application.Services;

public class BookingService : IBookingService
{
    private static readonly SemaphoreSlim CreateLock = new(1, 1);

    private readonly INotificationService _notificationService;
    private readonly IUserEmailService _userEmailService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IBookingRepository bookingRepository,IRoomRepository roomRepository
        ,IMapper mapper,INotificationService notificationService,
        IUserEmailService userEmailService,ILogger<BookingService> logger)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _mapper = mapper;
        _notificationService = notificationService;
        _userEmailService = userEmailService;
        _logger = logger;
    }

    

    public async Task<BookingDto> CreateAsync(
        string guestId,
        Guid idempotencyKey,
        CreateBookingDto dto)
    {
        if (string.IsNullOrWhiteSpace(guestId))
            throw new UnauthorizedAccessException("Guest identity could not be determined.");

        var checkIn = dto.CheckInDate.Date;
        var checkOut = dto.CheckOutDate.Date;

        await CreateLock.WaitAsync();

        try
        {
            var existingBooking = await _bookingRepository
                .GetByIdempotencyKeyAsync(guestId, idempotencyKey);

            if (existingBooking is not null)
            {
                var isSameRequest =
                    existingBooking.RoomId == dto.RoomId &&
                    existingBooking.CheckInDate == checkIn &&
                    existingBooking.CheckOutDate == checkOut;

                if (!isSameRequest)
                {
                    throw new ConflictException(
                        "This Idempotency-Key was already used for a different booking request.");
                }

                return _mapper.Map<BookingDto>(existingBooking);
            }

            if (checkIn < DateTime.UtcNow.Date)
            {
                throw new ConflictException("Check-in date cannot be in the past.");
            }

            if (checkOut <= checkIn)
            {
                throw new ConflictException("Check-out date must be after check-in date.");
            }

            var room =await _roomRepository.GetByIdWithRoomTypeAsync(dto.RoomId);

            if (room is null)
            {
                throw new NotFoundException("Room not found.");
            }

            if (!room.IsActive)
            {
                throw new ConflictException("This room is currently unavailable.");
            }

            var available =await _bookingRepository.IsRoomAvailableAsync(dto.RoomId,checkIn,checkOut);

            if (!available)
            {
                throw new ConflictException("The room is already booked for the selected dates.");
            }

            var numberOfNights =(checkOut - checkIn).Days;

            var totalPrice =numberOfNights *room.RoomType.PricePerNight;

            var booking = new Booking
            {
                GuestId = guestId,
                IdempotencyKey = idempotencyKey,
                RoomId = dto.RoomId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                NumberOfNights = numberOfNights,
                TotalPrice = totalPrice,
                Status = BookingStatus.Requested,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);

            await _bookingRepository.SaveChangesAsync();

            var createdBooking =await _bookingRepository.GetByIdWithDetailsAsync(booking.Id);

            await NotifyGuestAsync(
                booking,
                "Booking Request Received",
                $"Your booking request #{booking.Id} has been received for room {room.RoomNumber} from {checkIn:yyyy-MM-dd} to {checkOut:yyyy-MM-dd}.");

            return _mapper.Map<BookingDto>(createdBooking);
        }
        finally
        {
            CreateLock.Release();
        }
    }

    

    public async Task<IEnumerable<BookingDto>>GetMyBookingsAsync(string guestId)
    {
        var bookings =await _bookingRepository.GetByGuestIdAsync(guestId);

        return _mapper.Map<IEnumerable<BookingDto>>(bookings);
    }

    public async Task<BookingDto?> GetMyBookingByIdAsync(string guestId,int bookingId)
    {
        var booking =await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

        if (booking is null)
            return null;

        if (booking.GuestId != guestId)
        {
            throw new ForbiddenException("You can only access your own bookings.");
        }

        return _mapper.Map<BookingDto>(booking);
    }


    public async Task<bool>CancelMyBookingAsync(string guestId,int bookingId)
    {
        var booking =await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

        if (booking is null)
            return false;

        if (booking.GuestId != guestId)
        {
            throw new ForbiddenException("You can only cancel your own booking.");
        }

        if (booking.Status != BookingStatus.Requested &&
            booking.Status != BookingStatus.Confirmed)
        {
            throw new ConflictException("This booking cannot be cancelled.");
        }

        booking.Status =BookingStatus.Cancelled;

        _bookingRepository.Update(booking);

        await _bookingRepository.SaveChangesAsync();

        return true;
    }


    public async Task<PaginatedResultDto<BookingDto>> GetAllAsync(int pageNumber,int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var result = await _bookingRepository.GetPagedAsync(
            pageNumber,
            pageSize);

        var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(
            result.Items);

        var totalPages = (int)Math.Ceiling(
            result.TotalCount / (double)pageSize);

        return new PaginatedResultDto<BookingDto>
        {
            Items = bookingDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = totalPages
        };
    }
    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var booking =await _bookingRepository.GetByIdWithDetailsAsync(id);

        if (booking is null)
            return null;

        return _mapper.Map<BookingDto>(booking);
    }


    public async Task<bool> ConfirmAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Requested)
        {
            throw new ConflictException(
                "Only requested bookings can be confirmed.");
        }

        booking.Status = BookingStatus.Confirmed;

        _bookingRepository.Update(booking);

        await _bookingRepository.SaveChangesAsync();

        await NotifyGuestAsync(booking, "Booking Confirmed", $"Your booking #{booking.Id} has been confirmed successfully.");

        return true;
    }
    public async Task<bool> CancelAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Requested &&
            booking.Status != BookingStatus.Confirmed)
        {
            throw new ConflictException(
                "This booking cannot be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;

        _bookingRepository.Update(booking);

        await _bookingRepository.SaveChangesAsync();

        await NotifyGuestAsync(booking,"Booking Cancelled",$"Your booking #{booking.Id} has been cancelled.");

        return true;
    }
    public async Task<bool> CheckInAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new ConflictException("Only confirmed bookings can be checked in.");
        }

        if (!booking.Room.IsActive)
        {
            throw new ConflictException("The room is currently unavailable.");
        }

        booking.Status = BookingStatus.CheckedIn;

        _bookingRepository.Update(booking);

        await _bookingRepository.SaveChangesAsync();

        return true;
    }


    public async Task<bool> CheckOutAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);

        if (booking is null)
            throw new NotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.CheckedIn)
        {
            throw new ConflictException("Only checked-in bookings can be checked out.");
        }

        booking.Status = BookingStatus.CheckedOut;

        _bookingRepository.Update(booking);

        await _bookingRepository.SaveChangesAsync();

        return true;
    }

    

    public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateTime checkInDate,DateTime checkOutDate)
    {
        var checkIn = checkInDate.Date;
        var checkOut = checkOutDate.Date;

        if (checkIn < DateTime.UtcNow.Date)
        {
            throw new ConflictException("Check-in date cannot be in the past.");
        }

        if (checkOut <= checkIn)
        {
            throw new ConflictException("Check-out date must be after check-in date.");
        }

        var rooms =await _bookingRepository.GetAvailableRoomsAsync(checkIn,checkOut);

        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }

    private async Task NotifyGuestAsync(
        Booking booking,
        string subject,
        string message)
    {
        try
        {
            var recipient = await _userEmailService
                .GetEmailByUserIdAsync(booking.GuestId);

            if (string.IsNullOrWhiteSpace(recipient))
            {
                _logger.LogWarning(
                    "No email was found for guest {GuestId}; notification for booking {BookingId} was not sent.",
                    booking.GuestId,
                    booking.Id);
                return;
            }

            await _notificationService.SendAsync(recipient, subject, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unable to send notification for booking {BookingId} to guest {GuestId}.",
                booking.Id,
                booking.GuestId);
        }
    }
}
