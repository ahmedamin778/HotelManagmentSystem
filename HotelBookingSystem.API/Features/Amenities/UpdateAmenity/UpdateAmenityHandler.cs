using AutoMapper;
using HotelBookingSystem.Application.Exceptions;
using HotelBookingSystem.Application.Interfaces.IRepositories;

namespace HotelBookingSystem.Api.Features.Amenities.UpdateAmenity;

public class UpdateAmenityHandler
{
    private readonly IAmenityRepository _amenityRepository;
    private readonly IMapper _mapper;

    public UpdateAmenityHandler(IAmenityRepository amenityRepository, IMapper mapper)
    {
        _amenityRepository = amenityRepository;
        _mapper = mapper;
    }

    public async Task<bool> HandleAsync(UpdateAmenityCommand command)
    {
        var amenity = await _amenityRepository.GetByIdAsync(command.Id);

        if (amenity is null)
            throw new NotFoundException("Amenity not found.");

        var exists = await _amenityRepository.ExistsByNameAsync(command.Dto.Name);

        if (exists &&
            !amenity.Name.Equals(command.Dto.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                "An amenity with the same name already exists.");
        }

        _mapper.Map(command.Dto, amenity);
        _amenityRepository.Update(amenity);
        await _amenityRepository.SaveChangesAsync();

        return true;
    }
}
