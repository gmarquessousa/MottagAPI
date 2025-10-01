using App.Application.DTOs.Common;
using App.Application.DTOs.Motos;
using App.Application.Exceptions;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Application.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Services;

public class MotoService : IMotoService
{
    private readonly IRepository<Moto> _repo;
    private readonly IRepository<Patio> _patioRepo;
    private readonly IValidator<CreateMotoDto> _createValidator;
    private readonly IValidator<UpdateMotoDto> _updateValidator;
    private readonly IMapper _mapper;

    public MotoService(IRepository<Moto> repo, IRepository<Patio> patioRepo, IValidator<CreateMotoDto> cVal, IValidator<UpdateMotoDto> uVal, IMapper mapper)
    { _repo = repo; _patioRepo = patioRepo; _createValidator = cVal; _updateValidator = uVal; _mapper = mapper; }

    public async Task<MotoReadDto> CreateAsync(CreateMotoDto dto, CancellationToken ct = default)
    {
        var v = await _createValidator.ValidateAsync(dto, ct);
        if (!v.IsValid) throw new AppValidationException(v.Errors);
        var patioExists = await _patioRepo.Query().AnyAsync(p => p.Id == dto.PatioId, ct);
        if (!patioExists) throw new NotFoundException("Pátio não encontrado");
        var exists = await _repo.Query().AnyAsync(m => m.Placa == dto.Placa, ct);
        if (exists) throw new ConflictException($"Moto com placa '{dto.Placa}' já existe.");
        var entity = _mapper.Map<Moto>(dto);
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return _mapper.Map<MotoReadDto>(entity);
    }

    public async Task<MotoReadDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) throw new NotFoundException("Moto não encontrada");
        return _mapper.Map<MotoReadDto>(entity);
    }

    public async Task<PagedResultDto<MotoReadDto>> ListAsync(Guid? patioId, MotoStatus? status, string? placa, string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct = default)
    {
        var baseQuery = _repo.Query().Filter(patioId, status, placa);
        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery.OrderMotos(sortBy, sortDir).ApplyPaging(page, pageSize)
            .ProjectTo<MotoReadDto>(_mapper.ConfigurationProvider).ToListAsync(ct);
        return new PagedResultDto<MotoReadDto>{ Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<MotoReadDto> UpdateAsync(Guid id, UpdateMotoDto dto, CancellationToken ct = default)
    {
        var v = await _updateValidator.ValidateAsync(dto, ct);
        if (!v.IsValid) throw new AppValidationException(v.Errors);
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) throw new NotFoundException("Moto não encontrada");

        // Update não permite alterar Placa ou PatioId (não presentes no DTO). Modelo e Status apenas.
    _mapper.Map(dto, entity); // applies Modelo & Status
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);
        return _mapper.Map<MotoReadDto>(entity);
    }

    public async Task DeleteAsync(Guid id, string _ignored, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return;
        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);
    }
}