using App.Application.DTOs.Common;using App.Application.DTOs.Tags;using App.Application.Services;using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

/// <summary>Endpoints para gestão de Tags (identificação de motos).</summary>
[ApiController]
[Route("api/v1/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagService _service;private readonly ILinkBuilder _links;
    public TagsController(ITagService service, ILinkBuilder links){_service=service;_links=links;}

    /// <summary>Lista tags com filtro opcional por serial.</summary>
    /// <remarks>Exemplo: GET /api/v1/tags?page=1&amp;pageSize=10&amp;serial=XYZ</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TagReadDto>),200)]
    public async Task<ActionResult<PagedResultDto<TagReadDto>>> List([FromQuery] string? serial,[FromQuery] string? sortBy,[FromQuery] string? sortDir,[FromQuery] int page=1,[FromQuery] int pageSize=10, CancellationToken ct=default)
    {
        var result = await _service.ListAsync(serial, sortBy, sortDir, page, pageSize, ct);
        var withLinks = _links.WithCollectionLinks("tags", result, (p,ps)=>Url.ActionLink(nameof(List), values:new { serial, sortBy, sortDir, page=p,pageSize=ps})!);
        return Ok(withLinks);
    }

    /// <summary>Obtém uma tag por Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<TagReadDto>),200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ResourceDto<TagReadDto>>> Get(Guid id, CancellationToken ct=default)
    {
        var dto = await _service.GetAsync(id, ct);
        var resource = new ResourceDto<TagReadDto>{ Data = dto };
        _links.WithItemLinks("tags", id, resource);
        return Ok(resource);
    }

    /// <summary>Cria uma nova tag.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResourceDto<TagReadDto>),201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<ResourceDto<TagReadDto>>> Create([FromBody] CreateTagDto body, CancellationToken ct=default)
    {
        var dto = await _service.CreateAsync(body, ct);
        var resource = new ResourceDto<TagReadDto>{ Data = dto };
        _links.WithItemLinks("tags", dto.Id, resource);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, resource);
    }

    /// <summary>Atualiza uma tag existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResourceDto<TagReadDto>),200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(412)]
    public async Task<ActionResult<ResourceDto<TagReadDto>>> Update(Guid id, [FromBody] UpdateTagDto body, CancellationToken ct=default)
    {
        var dto = await _service.UpdateAsync(id, body, ct);
        var resource = new ResourceDto<TagReadDto>{ Data = dto };
        _links.WithItemLinks("tags", id, resource);
        return Ok(resource);
    }

    /// <summary>Remove (delete físico) uma tag.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct=default)
    {
        await _service.DeleteAsync(id, string.Empty, ct);
        return NoContent();
    }
}