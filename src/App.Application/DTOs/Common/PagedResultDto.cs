namespace App.Application.DTOs.Common;

/// <summary>Resultado paginado com metadados e links.</summary>
public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNext => (Page * PageSize) < Total;
    public bool HasPrev => Page > 1;
    public IList<LinkDto> Links { get; set; } = new List<LinkDto>();
}
