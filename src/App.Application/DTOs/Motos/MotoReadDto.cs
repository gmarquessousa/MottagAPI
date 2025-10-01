using App.Domain.Enums;

namespace App.Application.DTOs.Motos;

/// <summary>Representação de uma moto retornada pela API.</summary>
public class MotoReadDto
{
    /// <summary>Identificador único.</summary>
    public Guid Id { get; set; }
    /// <summary>Identificador do pátio associado.</summary>
    public Guid PatioId { get; set; }
    /// <summary>Placa da moto.</summary>
    public string Placa { get; set; } = string.Empty;
    /// <summary>Modelo.</summary>
    public string Modelo { get; set; } = string.Empty;
    /// <summary>Status atual.</summary>
    public MotoStatus Status { get; set; }
}
