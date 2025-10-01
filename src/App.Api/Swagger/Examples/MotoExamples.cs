using App.Application.DTOs.Motos;
using Swashbuckle.AspNetCore.Filters;
using App.Domain.Enums;

namespace App.Api.Swagger.Examples;

public class CreateMotoDtoExample : IExamplesProvider<CreateMotoDto>
{
    public CreateMotoDto GetExamples() => new()
    {
        PatioId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Placa = "ABC1D23",
        Modelo = "CG 160 Start",
    Status = MotoStatus.Disponivel
    };
}

public class MotoReadDtoExample : IExamplesProvider<MotoReadDto>
{
    public MotoReadDto GetExamples() => new()
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        PatioId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Placa = "ABC1D23",
        Modelo = "CG 160 Start",
    Status = MotoStatus.Disponivel
    };
}