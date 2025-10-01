using App.Domain.Enums;

namespace App.Application.DTOs.Tags;

/// <summary>Payload para criação de uma tag RFID.</summary>
public class CreateTagDto
{
    /// <summary>Identificador da moto associada (opcional no momento da criação).</summary>
    public Guid? MotoId { get; set; }
    /// <summary>Número de série único.</summary>
    public string Serial { get; set; } = string.Empty;
    /// <summary>Versão/Tipo da tag.</summary>
    public TagTipo Tipo { get; set; } = TagTipo.V1;
    /// <summary>Nível de bateria (0-100).</summary>
    public int BateriaPct { get; set; }
}
