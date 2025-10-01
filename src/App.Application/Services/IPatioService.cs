using App.Application.DTOs.Patios;
using App.Application.DTOs.Common;

namespace App.Application.Services;

public interface IPatioService
{
    Task<PatioReadDto> CreateAsync(CreatePatioDto dto, CancellationToken ct = default);
    Task<PatioReadDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<PagedResultDto<PatioReadDto>> ListAsync(string? search, string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct = default);
    Task<PatioReadDto> UpdateAsync(Guid id, UpdatePatioDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string ifMatch, CancellationToken ct = default);
}