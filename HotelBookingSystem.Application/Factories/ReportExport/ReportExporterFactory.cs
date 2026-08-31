using HotelBookingSystem.Application.Exceptions;

namespace HotelBookingSystem.Application.Factories.ReportExport;

public class ReportExporterFactory : IReportExporterFactory
{
    private readonly IReadOnlyDictionary<string, IReportExporter> _exporters;

    public ReportExporterFactory(IEnumerable<IReportExporter> exporters)
    {
        _exporters = exporters.ToDictionary(
            exporter => exporter.Format,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReportExporter Create(string format)
    {
        if (_exporters.TryGetValue(format, out var exporter))
            return exporter;

        throw new ConflictException(
            $"Unsupported export format '{format}'. Supported formats: {string.Join(", ", _exporters.Keys)}.");
    }
}
