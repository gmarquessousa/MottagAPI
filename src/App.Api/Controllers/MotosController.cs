using App.Application.DTOs.Common;using App.Application.DTOs.Motos;using App.Application.Services;using App.Domain.Enums;using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

/// <summary>Endpoints para gestão de Motos.</summary>
[ApiController]
[Route("api/v1/motos")]
public class MotosController : ControllerBase
{
    private readonly IMotoService _service;
    private readonly ILinkBuilder _links;
    public MotosController(IMotoService service, ILinkBuilder links)
    {
        _service=service;
        _links=links;
    }

    /// <summary>Lista motos com filtros opcionais.</summary>
    /// <remarks>Exemplo: GET /api/v1/motos?page=1&amp;pageSize=10&amp;placa=ABC1234</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<MotoReadDto>),200)]
    public async Task<ActionResult<PagedResultDto<MotoReadDto>>> List([FromQuery] Guid? patioId,[FromQuery] MotoStatus? status,[FromQuery] string? placa,[FromQuery] string? sortBy,[FromQuery] string? sortDir,[FromQuery] int page=1,[FromQuery] int pageSize=10, CancellationToken ct=default)
    {
        var result = await _service.ListAsync(patioId, status, placa, sortBy, sortDir, page, pageSize, ct);
        var withLinks = _links.WithCollectionLinks("motos", result, (p,ps)=>Url.ActionLink(nameof(List), values:new { patioId, status, placa, sortBy, sortDir, page=p,pageSize=ps})!);
        return Ok(withLinks);
    }

    /// <summary>Obtém uma moto por Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<MotoReadDto>),200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ResourceDto<MotoReadDto>>> Get(Guid id, CancellationToken ct=default)
    {
        var dto = await _service.GetAsync(id, ct);
        var resource = new ResourceDto<MotoReadDto>{ Data = dto };
        _links.WithItemLinks("motos", id, resource);
        return Ok(resource);
    }

    /// <summary>Cria uma nova moto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<MotoReadDto>),201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ResourceDto<MotoReadDto>>> Create([FromBody] CreateMotoDto body, CancellationToken ct=default)
    {
        var dto = await _service.CreateAsync(body, ct);
        var resource = new ResourceDto<MotoReadDto>{ Data = dto };
        _links.WithItemLinks("motos", dto.Id, resource);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, resource);
    }

    /// <summary>Atualiza uma moto existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<MotoReadDto>),200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(412)]
    public async Task<ActionResult<ResourceDto<MotoReadDto>>> Update(Guid id, [FromBody] UpdateMotoDto body, CancellationToken ct=default)
    {
        var dto = await _service.UpdateAsync(id, body, ct);
        var resource = new ResourceDto<MotoReadDto>{ Data = dto };
        _links.WithItemLinks("motos", id, resource);
        return Ok(resource);
    }

    /// <summary>Remove (delete físico) uma moto.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct=default)
    {
        await _service.DeleteAsync(id, string.Empty, ct);
        return NoContent();
    }
}