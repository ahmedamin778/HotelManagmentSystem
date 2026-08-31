namespace HotelBookingSystem.Application.Factories.ReportExport;

public interface IReportExporter
{
    string Format { get; }
    string ContentType { get; }
    string FileExtension { get; }
    string Export(object report);
}
