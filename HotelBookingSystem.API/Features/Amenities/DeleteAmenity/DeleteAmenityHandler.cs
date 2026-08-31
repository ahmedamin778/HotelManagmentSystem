using HotelBookingSystem.Application.Exceptions;
using HotelBookingSystem.Application.Interfaces.IRepositories;

namespace HotelBookingSystem.Api.Features.Amenities.DeleteAmenity;

public class DeleteAmenityHandler
{
    private readonly IAmenityRepository _amenityRepository;

    public DeleteAmenityHandler(IAmenityRepository amenityRepository)
    {
        _amenityRepository = amenityRepository;
    }

    public async Task<bool> HandleAsync(DeleteAmenityCommand command)
    {
        var amenity = await _amenityRepository.GetByIdAsync(command.Id);

        if (amenity is null)
            throw new NotFoundException("Amenity not found.");

        _amenityRepository.Delete(amenity);
        await _amenityRepository.SaveChangesAsync();

        return true;
    }
}
