using App.Domain.Enums;

namespace App.Application.DTOs.Motos;

/// <summary>Payload para atualização parcial/total de uma moto.</summary>
public class UpdateMotoDto
{
    /// <summary>Modelo da moto.</summary>
    public string Modelo { get; set; } = string.Empty;
    /// <summary>Status atual da moto.</summary>
    public MotoStatus Status { get; set; }
}
