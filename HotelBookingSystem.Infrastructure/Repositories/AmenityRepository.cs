using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Core.Models;
using HotelBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Infrastructure.Repositories;

public class AmenityRepository: GenericRepository<Amenity>, IAmenityRepository
{
    public AmenityRepository(ApplicationDbContext context): base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Amenities.AnyAsync(x => x.Name.ToLower() == name.ToLower());
    }

    public async Task<(IEnumerable<Amenity> Items, int TotalCount)> GetPagedAsync(int pageNumber,int pageSize)
    {
        var query = _context.Amenities
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