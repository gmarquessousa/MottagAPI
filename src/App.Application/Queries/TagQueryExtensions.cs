using App.Domain.Entities;

namespace App.Application.Queries;

public static class TagQueryExtensions
{
    public static IQueryable<Tag> Filter(this IQueryable<Tag> query, string? serial)
    {
        if (!string.IsNullOrWhiteSpace(serial))
        {
            serial = serial.Trim();
            query = query.Where(t => t.Serial == serial);
        }
        return query;
    }

    public static IQueryable<Tag> OrderTags(this IQueryable<Tag> query, string? sortBy, string? sortDir)
    {
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "serial" => desc ? query.OrderByDescending(t => t.Serial) : query.OrderBy(t => t.Serial),
            _ => desc ? query.OrderByDescending(t => t.Serial) : query.OrderBy(t => t.Serial)
        };
    }
}