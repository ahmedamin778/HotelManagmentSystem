using HotelBookingSystem.Application.DTOs.Amenities;

public class RoomTypeDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal PricePerNight { get; set; }

    public int MaxGuests { get; set; }

    public List<AmenityDto> Amenities { get; set; } = new();
}