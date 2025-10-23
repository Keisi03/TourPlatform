namespace TourPlatform.Application.Settings;

public class BulkUploadSettings
{
    public int BatchSize { get; set; }
    public int MaxWorkers { get; set; }
    public int ProgressIntervalSeconds { get; set; }
}
