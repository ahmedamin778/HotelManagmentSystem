using AutoMapper;
using HotelBookingSystem.Application.DTOs.Amenities;
using HotelBookingSystem.Application.Interfaces.IRepositories;

namespace HotelBookingSystem.Api.Features.Amenities.GetAmenityById;

public class GetAmenityByIdHandler
{
    private readonly IAmenityRepository _amenityRepository;
    private readonly IMapper _mapper;

    public GetAmenityByIdHandler(IAmenityRepository amenityRepository,IMapper mapper)
    {
        _amenityRepository = amenityRepository;
        _mapper = mapper;
    }

    public async Task<AmenityDto?> HandleAsync(GetAmenityByIdQuery query)
    {
        var amenity = await _amenityRepository.GetByIdAsync(query.Id);

        if (amenity is null)
            return null;

        return _mapper.Map<AmenityDto>(amenity);
    }
}