using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Core.Models;
using HotelBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Infrastructure.Repositories;

public class RoomTypeRepository: GenericRepository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(ApplicationDbContext context): base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.RoomTypes
            .AnyAsync(x => x.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<RoomType>> GetAllWithAmenitiesAsync()
    {
        return await _context.RoomTypes
            .Include(x => x.RoomTypeAmenities)
            .ThenInclude(x => x.Amenity)
            .ToListAsync();
    }

    public async Task<RoomType?> GetByIdWithAmenitiesAsync(int id)
    {
        return await _context.RoomTypes
            .Include(x => x.RoomTypeAmenities)
            .ThenInclude(x => x.Amenity)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<RoomType>> GetByAmenityIdAsync(int amenityId)
    {
        return await _context.RoomTypes
            .Include(x => x.RoomTypeAmenities)
            .ThenInclude(x => x.Amenity)
            .Where(x => x.RoomTypeAmenities.
            Any(x => x.AmenityId == amenityId))
            .ToListAsync();
    }

    public async Task<bool> AmenityExistsAsync(int amenityId)
    {
        return await _context.Amenities
            .AnyAsync(x => x.Id == amenityId);
    }

    public async Task<bool> RoomTypeAmenityExistsAsync(int roomTypeId,int amenityId)
    {
        return await _context.RoomTypeAmenities
            .AnyAsync(x =>
                x.RoomTypeId == roomTypeId &&
                x.AmenityId == amenityId);
    }

    public async Task AddAmenityAsync(int roomTypeId,int amenityId)
    {
        var roomTypeAmenity = new RoomTypeAmenity
        {
            RoomTypeId = roomTypeId,
            AmenityId = amenityId
        };

        await _context.RoomTypeAmenities.AddAsync(roomTypeAmenity);
    }

    public async Task<bool> RemoveAmenityAsync(int roomTypeId,int amenityId)
    {
        var roomTypeAmenity =
            await _context.RoomTypeAmenities
                .FirstOrDefaultAsync(x =>
                    x.RoomTypeId == roomTypeId &&
                    x.AmenityId == amenityId);

        if (roomTypeAmenity is null)
            return false;

        _context.RoomTypeAmenities.Remove(roomTypeAmenity);

        return true;
    }

    public async Task<(IEnumerable<RoomType> Items, int TotalCount)> GetPagedAsync(int pageNumber,int pageSize)
    {
        var query = _context.RoomTypes
            .Include(x => x.RoomTypeAmenities)
            .ThenInclude(x => x.Amenity)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}