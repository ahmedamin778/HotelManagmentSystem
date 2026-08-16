using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Interfaces.IRepositories;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<Review?> GetByBookingIdAsync(int bookingId);
    Task<IEnumerable<Review>> GetAllWithDetailsAsync();
    Task<Review?> GetByIdWithDetailsAsync(int ReviewId);
    Task<(IEnumerable<Review> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}