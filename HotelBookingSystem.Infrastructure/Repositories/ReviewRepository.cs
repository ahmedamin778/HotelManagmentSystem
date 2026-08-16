using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Core.Models;
using HotelBookingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingSystem.Infrastructure.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context): base(context)
    {
    }

    public async Task<Review?> GetByBookingIdAsync(int bookingId)
    {
        return await _context.Reviews
            .Include(x => x.Booking)
                .ThenInclude(x => x.Room)
                    .ThenInclude(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.BookingId == bookingId);
    }
    public async Task<Review?> GetByIdWithDetailsAsync(int ReviewId)
    {
        return await _context.Reviews.Include(x => x.Booking)
            .ThenInclude(x => x.Room)
            .ThenInclude(x => x.RoomType)
            .FirstOrDefaultAsync(x => x.Id == ReviewId);
    }

    public async Task<(IEnumerable<Review> Items, int TotalCount)> GetPagedAsync(int pageNumber,int pageSize)
    {
        var query = _context.Reviews
            .Include(x => x.Booking)
                .ThenInclude(x => x.Room)
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
    public async Task<IEnumerable<Review>> GetAllWithDetailsAsync()
    {
        return await _context.Reviews
            .Include(x => x.Booking)
                .ThenInclude(x => x.Room)
                    .ThenInclude(x => x.RoomType)
            .ToListAsync();
    }

}