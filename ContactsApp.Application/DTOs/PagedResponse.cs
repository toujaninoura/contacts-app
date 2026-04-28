namespace ContactsApp.Application.DTOs;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; init; } = Enumerable.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;

    public static PagedResponse<T> Create(IEnumerable<T> data, int page, int pageSize, int totalCount) =>
        new() { Data = data, Page = page, PageSize = pageSize, TotalCount = totalCount };
}
