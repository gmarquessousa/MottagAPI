using FluentValidation.Results;

namespace App.Application.Exceptions;

public class AppValidationException : Exception
{
    public IReadOnlyList<ValidationFailure> Errors { get; }
    public AppValidationException(IEnumerable<ValidationFailure> failures) : base("Validation failed")
    {
        Errors = failures.ToList();
    }
}