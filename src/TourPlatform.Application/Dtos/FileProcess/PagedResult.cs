namespace TourPlatform.Application.DTOS.FileProcess;

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public List<T> Items { get; set; } = new List<T>();
}
