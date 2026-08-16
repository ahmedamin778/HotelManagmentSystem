using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Core.Enums;
using HotelBookingSystem.Core.Models;
using HotelBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Infrastructure.Repositories;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context): base(context)
    {
    }



    public async Task<bool> IsRoomAvailableAsync(int roomId,DateTime checkInDate,DateTime checkOutDate)
    {
        var bookings = await _context.Bookings
            .Where(x =>
                x.RoomId == roomId &&
                x.Status != BookingStatus.Cancelled)
            .ToListAsync();

        foreach (var booking in bookings)
        {
            if (checkInDate < booking.CheckOutDate &&
                checkOutDate > booking.CheckInDate)
            {
                return false;
            }
        }

        return true;
    }
    /*
     return !await _context.Bookings.AnyAsync(x =>
    x.RoomId == roomId &&
    x.Status != BookingStatus.Cancelled &&
    (!excludedBookingId.HasValue ||
     x.Id != excludedBookingId.Value) &&
    checkInDate < x.CheckOutDate &&
    checkOutDate > x.CheckInDate);
     
     */

    public async Task<IEnumerable<Booking>> GetByGuestIdAsync(string guestId)
    {
        return await _context.Bookings
            .Include(x => x.Room)
            .ThenInclude(x => x.RoomType)
            .Where(x => x.GuestId == guestId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Bookings
            .Include(x => x.Room)
            .ThenInclude(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    
    public async Task<IEnumerable<Booking>> GetAllWithDetailsAsync()
    {
        return await _context.Bookings
            .Include(x => x.Room)
            .ThenInclude(x => x.RoomType)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

 

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkInDate,DateTime checkOutDate)
    {
        var bookedRoomIds = await _context.Bookings
            .Where(x =>
                x.Status != BookingStatus.Cancelled &&
                checkInDate < x.CheckOutDate &&
                checkOutDate > x.CheckInDate)
            .Select(x => x.RoomId)
            .ToListAsync();

        return await _context.Rooms
            .Include(x => x.RoomType).Where(x => x.IsActive && !bookedRoomIds.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<(IEnumerable<Booking> Items, int TotalCount)> GetPagedAsync(int pageNumber,int pageSize)
    {
        var query = _context.Bookings
            .Include(x => x.Room)
            .ThenInclude(x => x.RoomType)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}