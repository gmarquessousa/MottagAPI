using App.Application.DTOs.Common;
using App.Application.DTOs.Motos;
using App.Domain.Enums;

namespace App.Application.Services;

public interface IMotoService
{
    Task<MotoReadDto> CreateAsync(CreateMotoDto dto, CancellationToken ct = default);
    Task<MotoReadDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<PagedResultDto<MotoReadDto>> ListAsync(Guid? patioId, MotoStatus? status, string? placa, string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct = default);
    Task<MotoReadDto> UpdateAsync(Guid id, UpdateMotoDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string ifMatch, CancellationToken ct = default);
}