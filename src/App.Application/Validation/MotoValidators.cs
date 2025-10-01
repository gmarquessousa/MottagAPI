using App.Application.DTOs.Motos;
using FluentValidation;

namespace App.Application.Validation;

public class CreateMotoDtoValidator : AbstractValidator<CreateMotoDto>
{
    private const string PlacaPattern = @"^([A-Z]{3}[0-9]{4}|[A-Z]{3}[0-9][A-Z0-9][0-9]{2})$"; // antigo e Mercosul
    public CreateMotoDtoValidator()
    {
        RuleFor(x => x.PatioId).NotEmpty();
        RuleFor(x => x.Placa).NotEmpty().Length(7, 8).Matches(PlacaPattern).WithMessage("Placa inválida");
        RuleFor(x => x.Modelo).NotEmpty().MaximumLength(120);
        // Status opcional; se fornecido, deve ser enum válido (FluentValidation faz implicitamente)
    }
}

public class UpdateMotoDtoValidator : AbstractValidator<UpdateMotoDto>
{
    public UpdateMotoDtoValidator()
    {
        RuleFor(x => x.Modelo).NotEmpty().MaximumLength(120);
    }
}
