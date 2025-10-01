using App.Application.DTOs.Tags;
using FluentValidation;

namespace App.Application.Validation;

public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Serial).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BateriaPct).InclusiveBetween(0, 100);
    }
}

public class UpdateTagDtoValidator : AbstractValidator<UpdateTagDto>
{
    public UpdateTagDtoValidator()
    {
        RuleFor(x => x.BateriaPct).InclusiveBetween(0, 100);
    }
}
