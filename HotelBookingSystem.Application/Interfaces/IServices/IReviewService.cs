using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.DTOs.Reviews;

namespace HotelBookingSystem.Application.Interfaces.IServices;

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(string guestId, int bookingId, CreateReviewDto dto);

    Task<ReviewDto?> GetByIdAsync(int ReviewId);
    Task<ReviewDto?> GetByBookingIdAsync(int bookingId);
    Task<PaginatedResultDto<ReviewDto>> GetAllAsync(int pageNumber, int pageSize);
}