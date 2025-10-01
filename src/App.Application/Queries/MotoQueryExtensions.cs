using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Application.Queries;

public static class MotoQueryExtensions
{
    public static IQueryable<Moto> Filter(this IQueryable<Moto> query, Guid? patioId, MotoStatus? status, string? placa)
    {
        if (patioId.HasValue) query = query.Where(m => m.PatioId == patioId.Value);
        if (status.HasValue) query = query.Where(m => m.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(placa))
        {
            placa = placa.Trim();
            query = query.Where(m => m.Placa == placa);
        }
        return query;
    }

    public static IQueryable<Moto> OrderMotos(this IQueryable<Moto> query, string? sortBy, string? sortDir)
    {
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "placa" => desc ? query.OrderByDescending(m => m.Placa) : query.OrderBy(m => m.Placa),
            _ => desc ? query.OrderByDescending(m => m.Placa) : query.OrderBy(m => m.Placa)
        };
    }
}