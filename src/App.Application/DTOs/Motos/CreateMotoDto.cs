using App.Domain.Enums;

namespace App.Application.DTOs.Motos;

/// <summary>Payload para criação de uma moto.</summary>
public class CreateMotoDto
{
    /// <summary>Identificador do pátio onde a moto está localizada.</summary>
    public Guid PatioId { get; set; }
    /// <summary>Placa única da moto (formato validado).</summary>
    public string Placa { get; set; } = string.Empty;
    /// <summary>Modelo da moto.</summary>
    public string Modelo { get; set; } = string.Empty;
    /// <summary>Status da moto (opcional no create, default será atribuído se omisso).</summary>
    public MotoStatus? Status { get; set; }
}
