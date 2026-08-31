using System.Text.Json;

namespace HotelBookingSystem.Application.Factories.ReportExport;

public class JsonReportExporter : IReportExporter
{
    public string Format => "json";
    public string ContentType => "application/json";
    public string FileExtension => "json";

    public string Export(object report) =>
        JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
}
