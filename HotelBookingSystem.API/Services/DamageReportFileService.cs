using Microsoft.AspNetCore.Http;

namespace HotelBookingSystem.Api.Services;

public class DamageReportFileService
{
    private const long MaximumFileSize = 5 * 1024 * 1024;
    private const long StorageQuota = 50 * 1024 * 1024;

    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private readonly string _storageFolder;
    private readonly SemaphoreSlim _saveLock = new(1, 1);

    public DamageReportFileService(IWebHostEnvironment environment)
    {
        _storageFolder = Path.Combine(
            environment.ContentRootPath,
            "UploadedFiles",
            "DamageReports");
    }

    public async Task<DamageReportUploadResult> SaveAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
            throw new DamageReportValidationException("A file is required.");

        if (file.Length == 0)
            throw new DamageReportValidationException("The uploaded file is empty.");

        if (file.Length > MaximumFileSize)
            throw new DamageReportValidationException("The file exceeds the 5 MB size limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (extension is not ".jpg" and not ".jpeg" and not ".png")
            throw new DamageReportValidationException("Only JPG, JPEG, and PNG files are allowed.");

        await ValidateSignatureAsync(file, extension, cancellationToken);

        await _saveLock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_storageFolder);

            var currentSize = Directory
                .EnumerateFiles(_storageFolder)
                .Sum(path => new FileInfo(path).Length);

            if (currentSize + file.Length > StorageQuota)
            {
                throw new DamageReportQuotaExceededException(
                    "The 50 MB damage-report storage quota has been exceeded.");
            }

            var id = Guid.NewGuid();
            var storedFileName = $"{id:N}{extension}";
            var destinationPath = Path.Combine(_storageFolder, storedFileName);

            await using var source = file.OpenReadStream();
            await using var destination = new FileStream(
                destinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

            await source.CopyToAsync(destination, cancellationToken);

            return new DamageReportUploadResult(
                id,
                file.FileName,
                storedFileName,
                file.Length);
        }
        finally
        {
            _saveLock.Release();
        }
    }

    public DamageReportDownload? Find(Guid id)
    {
        var safeId = id.ToString("N");

        foreach (var (extension, contentType) in AllowedDownloadTypes)
        {
            var path = Path.Combine(_storageFolder, safeId + extension);

            if (File.Exists(path))
                return new DamageReportDownload(path, contentType, safeId + extension);
        }

        return null;
    }

    private static async Task ValidateSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        var expectedSignature = extension == ".png" ? PngSignature : JpegSignature;
        var header = new byte[expectedSignature.Length];

        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header, cancellationToken);

        if (bytesRead != expectedSignature.Length ||
            !header.AsSpan().SequenceEqual(expectedSignature))
        {
            throw new DamageReportValidationException(
                "The file content does not match its image extension.");
        }
    }

    private static readonly (string Extension, string ContentType)[] AllowedDownloadTypes =
    [
        (".jpg", "image/jpeg"),
        (".jpeg", "image/jpeg"),
        (".png", "image/png")
    ];
}

public record DamageReportUploadResult(
    Guid Id,
    string OriginalFileName,
    string StoredFileName,
    long Size);

public record DamageReportDownload(
    string Path,
    string ContentType,
    string DownloadName);

public class DamageReportValidationException(string message) : Exception(message);

public class DamageReportQuotaExceededException(string message) : Exception(message);
