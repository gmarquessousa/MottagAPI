using App.Application.DTOs.Common;
using App.Application.DTOs.Tags;
using App.Application.Exceptions;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Application.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Services;

public class TagService : ITagService
{
    private readonly IRepository<Tag> _repo;
    private readonly IRepository<Moto> _motoRepo;
    private readonly IValidator<CreateTagDto> _createValidator;
    private readonly IValidator<UpdateTagDto> _updateValidator;
    private readonly IMapper _mapper;

    public TagService(IRepository<Tag> repo, IRepository<Moto> motoRepo, IValidator<CreateTagDto> cVal, IValidator<UpdateTagDto> uVal, IMapper mapper)
    { _repo = repo; _motoRepo = motoRepo; _createValidator = cVal; _updateValidator = uVal; _mapper = mapper; }

    public async Task<TagReadDto> CreateAsync(CreateTagDto dto, CancellationToken ct = default)
    {
        var v = await _createValidator.ValidateAsync(dto, ct);
        if (!v.IsValid) throw new AppValidationException(v.Errors);
        var existsSerial = await _repo.Query().AnyAsync(t => t.Serial == dto.Serial, ct);
        if (existsSerial) throw new ConflictException($"Tag com serial '{dto.Serial}' já existe.");
        if (dto.MotoId.HasValue)
        {
            var motoExists = await _motoRepo.Query().AnyAsync(m => m.Id == dto.MotoId.Value, ct);
            if (!motoExists) throw new NotFoundException("Moto não encontrada");
            // one tag per moto rule: check no tag assigned to that moto
            var tagAlready = await _repo.Query().AnyAsync(t => t.MotoId == dto.MotoId.Value, ct);
            if (tagAlready) throw new ConflictException("Já existe uma tag associada a esta moto.");
        }
        var entity = _mapper.Map<Tag>(dto);
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return _mapper.Map<TagReadDto>(entity);
    }

    public async Task<TagReadDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) throw new NotFoundException("Tag não encontrada");
        return _mapper.Map<TagReadDto>(entity);
    }

    public async Task<PagedResultDto<TagReadDto>> ListAsync(string? serial, string? sortBy, string? sortDir, int page, int pageSize, CancellationToken ct = default)
    {
        var baseQuery = _repo.Query().Filter(serial);
        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery.OrderTags(sortBy, sortDir).ApplyPaging(page, pageSize)
            .ProjectTo<TagReadDto>(_mapper.ConfigurationProvider).ToListAsync(ct);
        return new PagedResultDto<TagReadDto>{ Items=items, Total=total, Page=page, PageSize=pageSize };
    }

    public async Task<TagReadDto> UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default)
    {
        var v = await _updateValidator.ValidateAsync(dto, ct);
        if (!v.IsValid) throw new AppValidationException(v.Errors);
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) throw new NotFoundException("Tag não encontrada");

        // Moto reassignment: allow if rule satisfied (only one tag per moto)
        if (entity.MotoId != dto.MotoId)
        {
            if (dto.MotoId.HasValue)
            {
                var motoExists = await _motoRepo.Query().AnyAsync(m => m.Id == dto.MotoId.Value, ct);
                if (!motoExists) throw new NotFoundException("Moto não encontrada");
                var tagAlready = await _repo.Query().AnyAsync(t => t.MotoId == dto.MotoId.Value && t.Id != id, ct);
                if (tagAlready) throw new ConflictException("Já existe uma tag associada a esta moto.");
            }
        }
        _mapper.Map(dto, entity);
        _repo.Update(entity);
        await _repo.SaveChangesAsync(ct);
        return _mapper.Map<TagReadDto>(entity);
    }

    public async Task DeleteAsync(Guid id, string _ignored, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity == null) return;
        _repo.Remove(entity);
        await _repo.SaveChangesAsync(ct);
    }
}