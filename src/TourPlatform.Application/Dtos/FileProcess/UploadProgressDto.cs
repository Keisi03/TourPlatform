namespace TourPlatform.Application.Dtos.FileProcess;

public class UploadProgressDto
{
    public int Processed { get; set; }
    public int Inserted { get; set; }
    public int Errors { get; set; }
    public int PercentComplete { get; set; }
}
