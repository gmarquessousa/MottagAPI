namespace App.Application.DTOs.Patios;

/// <summary>Dados para criação de um pátio.</summary>
public class CreatePatioDto
{
    /// <summary>Nome legível do pátio. Deve ser único de forma lógica no contexto de negócio.</summary>
    public string Nome { get; set; } = string.Empty;
    /// <summary>Cidade onde o pátio está localizado.</summary>
    public string Cidade { get; set; } = string.Empty;
    /// <summary>Estado (UF) do pátio.</summary>
    public string Estado { get; set; } = string.Empty;
    /// <summary>País do pátio (ex: BR).</summary>
    public string Pais { get; set; } = string.Empty;
    /// <summary>Área em metros quadrados.</summary>
    public double AreaM2 { get; set; } = 0d;
}
