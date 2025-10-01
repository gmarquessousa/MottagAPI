using App.Application.DTOs.Patios;
using FluentValidation;

namespace App.Application.Validation;

public class CreatePatioDtoValidator : AbstractValidator<CreatePatioDto>
{
    public CreatePatioDtoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cidade).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Estado).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Pais).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AreaM2).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePatioDtoValidator : AbstractValidator<UpdatePatioDto>
{
    public UpdatePatioDtoValidator()
    {
        Include(new CreatePatioDtoValidator());
    }
}
