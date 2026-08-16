using AutoMapper;
using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.DTOs.Reviews;
using HotelBookingSystem.Application.Exceptions;
using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Application.Interfaces.IServices;
using HotelBookingSystem.Core.Enums;
using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public ReviewService(IReviewRepository reviewRepository,IBookingRepository bookingRepository,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<ReviewDto> CreateAsync(string guestId,int bookingId,CreateReviewDto dto)
    {
        var booking =
            await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

        if (booking is null)
        {
            throw new NotFoundException("Booking not found.");
        }

        if (booking.GuestId != guestId)
        {
            throw new ForbiddenException(
                "You can only review your own booking.");
        }

        // Review only after checkout
        if (booking.Status != BookingStatus.CheckedOut)
        {
            throw new ConflictException(
                "You can only review a completed stay.");
        }

        // One review per booking
        var existingReview =await _reviewRepository.GetByBookingIdAsync(bookingId);

        if (existingReview is not null)
        {
            throw new ConflictException("This booking has already been reviewed.");
        }

        var review = new Review
        {
            BookingId = bookingId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review);

        await _reviewRepository.SaveChangesAsync();

        var createdReview =await _reviewRepository.GetByIdWithDetailsAsync(review.Id);

        return _mapper.Map<ReviewDto>(createdReview);
    }

    public async Task<ReviewDto?> GetByIdAsync(int ReviewId)
    {
        var review =
            await _reviewRepository.GetByIdWithDetailsAsync(ReviewId);

        if (review is null)
            return null;

        return _mapper.Map<ReviewDto>(review);
    }

    public async Task<ReviewDto?> GetByBookingIdAsync(int bookingId)
    {
        var review =await _reviewRepository.GetByBookingIdAsync(bookingId);

        if (review is null)
            return null;

        return _mapper.Map<ReviewDto>(review);
    }

    public async Task<PaginatedResultDto<ReviewDto>> GetAllAsync(int pageNumber,int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var result =await _reviewRepository.GetPagedAsync(pageNumber,pageSize);

        var reviewDtos =_mapper.Map<IEnumerable<ReviewDto>>(result.Items);

        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

        return new PaginatedResultDto<ReviewDto>
        {
            Items = reviewDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = totalPages
        };
    }
}