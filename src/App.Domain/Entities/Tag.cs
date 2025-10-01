using App.Domain.Enums;

namespace App.Domain.Entities;

/// <summary>Tag de RFID associável a (no máximo) uma moto.</summary>
public class Tag : BaseEntity
{
    public Guid? MotoId { get; set; }
    public string Serial { get; set; } = string.Empty; // Único
    public TagTipo Tipo { get; set; } = TagTipo.V1;
    public int BateriaPct { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }

    // Navegação
    public Moto? Moto { get; set; }
}
