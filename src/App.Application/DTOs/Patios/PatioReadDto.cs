namespace App.Application.DTOs.Patios;

/// <summary>Representação de um pátio retornado pela API.</summary>
public class PatioReadDto
{
    /// <summary>Identificador único.</summary>
    public Guid Id { get; set; }
    /// <summary>Nome do pátio.</summary>
    public string Nome { get; set; } = string.Empty;
    /// <summary>Cidade.</summary>
    public string Cidade { get; set; } = string.Empty;
    /// <summary>Estado (UF).</summary>
    public string Estado { get; set; } = string.Empty;
    /// <summary>País.</summary>
    public string Pais { get; set; } = string.Empty;
    /// <summary>Área em metros quadrados.</summary>
    public double AreaM2 { get; set; } = 0d;
}
