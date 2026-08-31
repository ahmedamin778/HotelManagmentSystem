namespace HotelBookingSystem.Application.Factories.ReportExport;

public interface IReportExporterFactory
{
    IReportExporter Create(string format);
}
