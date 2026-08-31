using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Core.Models;
using HotelBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using HotelBookingSystem.Application.Strategies.RoomSorting;

namespace HotelBookingSystem.Infrastructure.Repositories;

public class RoomRepository: GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context): base(context)
    {
    }

    public async Task<bool>ExistsByRoomNumberAsync(string roomNumber)
    {
        return await _context.Rooms
            .AnyAsync(x => x.RoomNumber.ToLower() == roomNumber.ToLower());
    }

    public async Task<bool>RoomTypeExistsAsync(int roomTypeId)
    {
        return await _context.RoomTypes.AnyAsync(x => x.Id == roomTypeId);
    }

    public async Task<IEnumerable<Room>>GetAllWithRoomTypeAsync()
    {
        return await _context.Rooms
            .Include(x => x.RoomType)
            .Include(x => x.Bookings)
            .ToListAsync();
    }

    public async Task<Room?>GetByIdWithRoomTypeAsync(int id)
    {
        return await _context.Rooms
            .Include(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Room>>GetAllWithRoomTypeAndBookingsAsync(int roomTypeId)
    {
        return await _context.Rooms
            .Include(x => x.RoomType)
            .Include(x => x.Bookings)
            .ThenInclude(x => x.Review)
            .Where(x => x.RoomTypeId == roomTypeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetActiveRoomsAsync()
    {
        return await _context.Rooms.Include(x => x.RoomType).Where(x => x.IsActive)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Room> Items, int TotalCount)> GetPagedAsync(int pageNumber,int pageSize,
    int? roomTypeId = null,
    bool? isActive = null,
    IRoomSortingStrategy? sortingStrategy = null)
    {
        var query = _context.Rooms.Include(x => x.RoomType)
            .AsQueryable();

        // Filter by RoomType
        if (roomTypeId.HasValue)
        {
            query = query.Where(x => x.RoomTypeId == roomTypeId.Value);
        }

        // Filter by Active Status
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        query = sortingStrategy?.Apply(query) ?? query.OrderBy(x => x.RoomNumber);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
