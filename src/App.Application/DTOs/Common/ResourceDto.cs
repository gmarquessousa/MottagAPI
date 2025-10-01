namespace App.Application.DTOs.Common;

/// <summary>Envelope de recurso com links.</summary>
public class ResourceDto<T>
{
    public T? Data { get; set; }
    public IList<LinkDto> Links { get; set; } = new List<LinkDto>();
}
