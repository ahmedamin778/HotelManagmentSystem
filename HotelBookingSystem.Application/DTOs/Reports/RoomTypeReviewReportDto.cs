namespace HotelBookingSystem.Application.DTOs.Reports;

public class RoomTypeReviewReportDto
{
    public string RoomTypeName { get; set; } = string.Empty;

    public decimal AverageRating { get; set; }

    public int ReviewCount { get; set; }
}