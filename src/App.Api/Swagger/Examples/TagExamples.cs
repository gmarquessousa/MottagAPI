using App.Application.DTOs.Tags;
using Swashbuckle.AspNetCore.Filters;
using App.Domain.Enums;

namespace App.Api.Swagger.Examples;

public class CreateTagDtoExample : IExamplesProvider<CreateTagDto>
{
    public CreateTagDto GetExamples() => new()
    {
        MotoId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Serial = "TAG-0001",
        Tipo = TagTipo.V1,
        BateriaPct = 90
    };
}

public class TagReadDtoExample : IExamplesProvider<TagReadDto>
{
    public TagReadDto GetExamples() => new()
    {
        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        MotoId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Serial = "TAG-0001",
        Tipo = TagTipo.V1,
        BateriaPct = 90,
        LastSeenAt = DateTimeOffset.UtcNow
    };
}