using AutoMapper;
using HotelBookingSystem.Application.DTOs.Amenities;
using HotelBookingSystem.Application.DTOs.Common;
using HotelBookingSystem.Application.Interfaces.IRepositories;

namespace HotelBookingSystem.Api.Features.Amenities.GetAmenities;

public class GetAmenitiesHandler
{
    private readonly IAmenityRepository _amenityRepository;
    private readonly IMapper _mapper;

    public GetAmenitiesHandler(IAmenityRepository amenityRepository, IMapper mapper)
    {
        _amenityRepository = amenityRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<AmenityDto>> HandleAsync(GetAmenitiesQuery query)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;

        var result = await _amenityRepository.GetPagedAsync(pageNumber, pageSize);
        var amenityDtos = _mapper.Map<IEnumerable<AmenityDto>>(result.Items);
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

        return new PaginatedResultDto<AmenityDto>
        {
            Items = amenityDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount,
            TotalPages = totalPages
        };
    }
}
