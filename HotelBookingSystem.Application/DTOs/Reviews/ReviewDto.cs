namespace HotelBookingSystem.Application.DTOs.Reviews;

public class ReviewDto
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string RoomTypeName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}