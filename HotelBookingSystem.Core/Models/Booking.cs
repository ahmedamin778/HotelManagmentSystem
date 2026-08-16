using HotelBookingSystem.Core.Enums;

namespace HotelBookingSystem.Core.Models;

public class Booking
{
    public int Id { get; set; }

    public string GuestId { get; set; } = string.Empty;

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public int NumberOfNights { get; set; }

    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Requested;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Review? Review { get; set; }
}