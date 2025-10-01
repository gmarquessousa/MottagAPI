using App.Application.DTOs.Common;using App.Application.DTOs.Patios;using App.Application.Services;using Microsoft.AspNetCore.Mvc;using AutoMapper;

namespace App.Api.Controllers;

/// <summary>Endpoints para gestão de Pátios.</summary>
[ApiController]
[Route("api/v1/patios")]
public class PatiosController : ControllerBase
{
    private readonly IPatioService _service;private readonly IMapper _mapper;private readonly ILinkBuilder _links;
    public PatiosController(IPatioService service, IMapper mapper, ILinkBuilder links){_service=service;_mapper=mapper;_links=links;}

    /// <summary>Lista pátios com paginação e filtros simples.</summary>
    /// <remarks>Exemplo: GET /api/v1/patios?page=1&amp;pageSize=10</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<PatioReadDto>),200)]
    public async Task<ActionResult<PagedResultDto<PatioReadDto>>> List([FromQuery] string? search,[FromQuery] string? sortBy,[FromQuery] string? sortDir,[FromQuery] int page=1,[FromQuery] int pageSize=10, CancellationToken ct=default)
    {
        var result = await _service.ListAsync(search, sortBy, sortDir, page, pageSize, ct);
        // Wrap em ResourceDto? Para coleção já retornamos PagedResultDto com links
        var withLinks = _links.WithCollectionLinks("patios", result, (p,ps)=>Url.ActionLink(nameof(List), values:new { page=p, pageSize=ps, search, sortBy, sortDir})!);
        return Ok(withLinks);
    }

    /// <summary>Obtém um pátio por Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<PatioReadDto>),200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ResourceDto<PatioReadDto>>> Get(Guid id, CancellationToken ct=default)
    {
        var dto = await _service.GetAsync(id, ct);
        var resource = new ResourceDto<PatioReadDto>{ Data = dto };
        _links.WithItemLinks("patios", id, resource);
        return Ok(resource);
    }

    /// <summary>Cria um novo pátio.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<PatioReadDto>),201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ResourceDto<PatioReadDto>>> Create([FromBody] CreatePatioDto body, CancellationToken ct=default)
    {
        var dto = await _service.CreateAsync(body, ct);
        var resource = new ResourceDto<PatioReadDto>{ Data = dto };
        _links.WithItemLinks("patios", dto.Id, resource);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, resource);
    }

    /// <summary>Atualiza um pátio existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<PatioReadDto>),200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(412)]
    public async Task<ActionResult<ResourceDto<PatioReadDto>>> Update(Guid id, [FromBody] UpdatePatioDto body, CancellationToken ct=default)
    {
        // If-Match: cliente envia body.RowVersion já; ETag via header é opcional aqui pois RowVersion do DTO usado.
        var dto = await _service.UpdateAsync(id, body, ct);
        var resource = new ResourceDto<PatioReadDto>{ Data = dto };
        _links.WithItemLinks("patios", id, resource);
        return Ok(resource);
    }

    /// <summary>Remove (delete físico) um pátio.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct=default)
    {
        await _service.DeleteAsync(id, string.Empty, ct); // ignorando token de concorrência
        return NoContent();
    }
}