namespace Qms.Api.DTOs;

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int AllCount { get; set; }
    public int TotalPages { get; set; }
    public int ActiveCount { get; set; }
    public int ObsoleteCount { get; set; }
}
