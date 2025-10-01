using App.Application.DTOs.Patios;
using Swashbuckle.AspNetCore.Filters;

namespace App.Api.Swagger.Examples;

public class CreatePatioDtoExample : IExamplesProvider<CreatePatioDto>
{
    public CreatePatioDto GetExamples() => new()
    {
        Nome = "Central",
        Cidade = "São Paulo",
        Estado = "SP",
        Pais = "BR",
        AreaM2 = 1200
    };
}

public class PatioReadDtoExample : IExamplesProvider<PatioReadDto>
{
    public PatioReadDto GetExamples() => new()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Nome = "Central",
        Cidade = "São Paulo",
        Estado = "SP",
        Pais = "BR",
        AreaM2 = 1200
    };
}