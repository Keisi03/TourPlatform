namespace TourPlatform.Application.Contracts;

public interface IFileProcessorService
{
    Task ProcessCsvAsync(Stream file, int tourOperatorId, string connectionId, CancellationToken cancellationToken);
}
