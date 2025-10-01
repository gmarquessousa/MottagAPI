using App.Domain.Enums;

namespace App.Application.DTOs.Tags;

/// <summary>Representação de uma tag RFID retornada pela API.</summary>
public class TagReadDto
{
    /// <summary>Identificador único.</summary>
    public Guid Id { get; set; }
    /// <summary>Identificador da moto associada (se existir).</summary>
    public Guid? MotoId { get; set; }
    /// <summary>Número de série.</summary>
    public string Serial { get; set; } = string.Empty;
    /// <summary>Tipo da tag.</summary>
    public TagTipo Tipo { get; set; }
    /// <summary>Percentual de bateria.</summary>
    public int BateriaPct { get; set; }
    /// <summary>Instante da última leitura conhecida.</summary>
    public DateTimeOffset? LastSeenAt { get; set; }
}
