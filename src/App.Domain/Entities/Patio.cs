using App.Domain.Enums;

namespace App.Domain.Entities;

/// <summary>Pátio que contém motos.</summary>
public class Patio : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public double AreaM2 { get; set; }
        = 0d;

    // Navegações simplificadas
    public ICollection<Moto> Motos { get; set; } = new List<Moto>();
}
