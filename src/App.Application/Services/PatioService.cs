using App.Application.DTOs.Common;
using App.Application.DTOs.Patios;
using App.Application.Exceptions;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Application.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Services;

public class PatioService : IPatioService
{
    private readonly IRepository<Patio> _repo;
    private readonly IValidator<CreatePatioDto> _createValidator;
    private readonly IValidator<UpdatePatioDto> _updateValidator;
    private readonly IMapper _mapper;

    public PatioService(IRepository<Patio> repo, IValidator<CreatePatioDto> createValidator, IValidator<UpdatePatioDto> updateValidator, IMapper mapper)
    {
        _repo = repo; _createValidator = createValidator; _updateValidator = updateValidator; _mapper = mapper;
    }

    public async Task<PatioReadDto> CreateAsync(CreatePatioDto dto, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) throw new AppValidationException(validation.Errors);

        // Regra: Nome único
        var exists = await _repo.Query().OfType<Patio>().AnyAsync(p => p.Nome == dto.Nome, ct);
        if (exists) throw new ConflictException($"Patio com nome '{dto.Nome}' já existe.");

        var entity = _mapper.Map<Patio>(dto);
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return _mapper.Map<PatioReadDto>(entity);
    }

    public async Task<PatioReadDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) throw new NotFoundException("Pátio não encontrado");
        return _mapper.Map<PatioReadDto>(entity);
    }

    public async Task<PagedResultDto<PatioReadDto>> ListAsync(string? search, string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct = default)
    {
        var baseQuery = _repo.Query().OfType<Patio>().SearchByNome(search);
        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderPatios(sortBy, sortDir)
            .ApplyPaging(page, pageSize)
            .ProjectTo<PatioReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResultDto<PatioReadDto>{ Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<PatioReadDto> UpdateAsync(Guid id, UpdatePatioDto dto, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid) throw new AppValidationException(validation.Errors);

        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) throw new NotFoundException("Pátio não encontrado");

        // Nome único (se alterado)
        if (!string.Equals(entity.Nome, dto.Nome, StringComparison.Ordinal))
        {
            var exists = await _repo.Query().OfType<Patio>().AnyAsync(p => p.Nome == dto.Nome && p.Id != id, ct);
            if (exists) throw new ConflictException($"Patio com nome '{dto.Nome}' já existe.");
        }

        _mapper.Map(dto, entity); // aplica alterações
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);
        return _mapper.Map<PatioReadDto>(entity);
    }

    public async Task DeleteAsync(Guid id, string _ignored, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return; // idempotente
        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);
    }
}