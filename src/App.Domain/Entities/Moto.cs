using App.Domain.Enums;

namespace App.Domain.Entities;

/// <summary>Motocicleta rastreada.</summary>
public class Moto : BaseEntity
{
    public Guid PatioId { get; set; }
    public string Placa { get; set; } = string.Empty; // Única
    public string Modelo { get; set; } = string.Empty;
    public MotoStatus Status { get; set; } = MotoStatus.Disponivel;

    // Navegações
    public Patio? Patio { get; set; }
    public Tag? Tag { get; set; }
}
