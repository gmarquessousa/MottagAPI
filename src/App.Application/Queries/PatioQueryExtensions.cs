using App.Domain.Entities;

namespace App.Application.Queries;

public static class PatioQueryExtensions
{
    public static IQueryable<Patio> SearchByNome(this IQueryable<Patio> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        search = search.Trim();
        return query.Where(p => p.Nome.Contains(search));
    }

    public static IQueryable<Patio> OrderPatios(this IQueryable<Patio> query, string? sortBy, string? sortDir)
    {
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "nome" => desc ? query.OrderByDescending(p => p.Nome) : query.OrderBy(p => p.Nome),
            _ => desc ? query.OrderByDescending(p => p.Nome) : query.OrderBy(p => p.Nome)
        };
    }
}