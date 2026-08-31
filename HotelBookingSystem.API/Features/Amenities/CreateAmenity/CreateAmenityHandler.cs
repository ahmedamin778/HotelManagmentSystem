using AutoMapper;
using HotelBookingSystem.Application.DTOs.Amenities;
using HotelBookingSystem.Application.Exceptions;
using HotelBookingSystem.Application.Interfaces.IRepositories;
using HotelBookingSystem.Core.Models;

namespace HotelBookingSystem.Api.Features.Amenities.CreateAmenity;

public class CreateAmenityHandler
{
    private readonly IAmenityRepository _amenityRepository;
    private readonly IMapper _mapper;

    public CreateAmenityHandler(IAmenityRepository amenityRepository, IMapper mapper)
    {
        _amenityRepository = amenityRepository;
        _mapper = mapper;
    }

    public async Task<AmenityDto> HandleAsync(CreateAmenityCommand command)
    {
        var exists = await _amenityRepository.ExistsByNameAsync(command.Dto.Name);

        if (exists)
        {
            throw new ConflictException(
                "An amenity with the same name already exists.");
        }

        var amenity = _mapper.Map<Amenity>(command.Dto);

        await _amenityRepository.AddAsync(amenity);
        await _amenityRepository.SaveChangesAsync();

        return _mapper.Map<AmenityDto>(amenity);
    }
}
