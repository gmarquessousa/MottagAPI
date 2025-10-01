namespace App.Infrastructure.Queries;

public static class PagingExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;
        var skip = (page - 1) * pageSize;
        return query.Skip(skip).Take(pageSize);
    }
}
