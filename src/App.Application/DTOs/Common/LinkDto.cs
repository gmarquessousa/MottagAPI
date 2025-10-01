namespace App.Application.DTOs.Common;

/// <summary>Representa um link HATEOAS.</summary>
public record LinkDto(string Rel, string Href, string Method, bool Templated = false);
