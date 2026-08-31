using System.Text;

namespace HotelBookingSystem.Application.Factories.ReportExport;

public class TextReportExporter : IReportExporter
{
    public string Format => "text";
    public string ContentType => "text/plain";
    public string FileExtension => "txt";

    public string Export(object report)
    {
        var content = new StringBuilder();

        foreach (var property in report.GetType().GetProperties())
            content.AppendLine($"{property.Name}: {property.GetValue(report)}");

        return content.ToString();
    }
}
