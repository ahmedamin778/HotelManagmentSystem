namespace HotelBookingSystem.Application.DTOs.Bookings;

public class BookingDto
{
    public int Id { get; set; }

    public string GuestId { get; set; } = string.Empty;

    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string RoomTypeName { get; set; } = string.Empty;

    public decimal PricePerNight { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public int NumberOfNights { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}