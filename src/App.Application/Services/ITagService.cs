using App.Application.DTOs.Common;
using App.Application.DTOs.Tags;

namespace App.Application.Services;

public interface ITagService
{
    Task<TagReadDto> CreateAsync(CreateTagDto dto, CancellationToken ct = default);
    Task<TagReadDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<PagedResultDto<TagReadDto>> ListAsync(string? serial, string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct = default);
    Task<TagReadDto> UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string ifMatch, CancellationToken ct = default);
}