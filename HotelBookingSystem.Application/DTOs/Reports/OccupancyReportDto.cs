namespace HotelBookingSystem.Application.DTOs.Reports;

public class OccupancyReportDto
{
    public int TotalActiveRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public decimal OccupancyPercentage { get; set; }
}