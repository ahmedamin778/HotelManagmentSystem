using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Application.DTOs.Bookings;

public class CreateBookingDto
{
    [Required]
    public int RoomId { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }
}