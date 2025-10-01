using System.ComponentModel.DataAnnotations;

namespace App.Domain.Entities;

/// <summary>
/// Entidade base simplificada (apenas Id) para reduzir complexidade conforme requisitos da entrega.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
