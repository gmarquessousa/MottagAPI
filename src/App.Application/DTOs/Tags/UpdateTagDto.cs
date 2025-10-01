using App.Domain.Enums;

namespace App.Application.DTOs.Tags;

/// <summary>Payload para atualização de uma tag.</summary>
public class UpdateTagDto
{
    /// <summary>Identificador opcional da moto associada.</summary>
    public Guid? MotoId { get; set; }
    /// <summary>Tipo/Versão da tag.</summary>
    public TagTipo Tipo { get; set; } = TagTipo.V1;
    /// <summary>Nível de bateria atual.</summary>
    public int BateriaPct { get; set; }
}
