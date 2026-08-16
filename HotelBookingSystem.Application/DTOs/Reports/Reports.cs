public class DashboardReportDto
{
    public int TotalRooms { get; set; }
    public int ActiveRooms { get; set; }
    public int InactiveRooms { get; set; }

    public int TotalBookings { get; set; }
    public int RequestedBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CheckedInBookings { get; set; }
    public int CheckedOutBookings { get; set; }
    public int CancelledBookings { get; set; }

    public decimal TotalRevenue { get; set; }
}