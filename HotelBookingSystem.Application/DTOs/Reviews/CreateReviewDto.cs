using System.ComponentModel.DataAnnotations;

namespace HotelBookingSystem.Application.DTOs.Reviews;

public class CreateReviewDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}