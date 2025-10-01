using App.Application.DTOs.Common;

namespace App.Application.Services;

public interface ILinkBuilder
{
    ResourceDto<T> WithItemLinks<T>(string resourceName, Guid id, ResourceDto<T> resource) where T : class;
    PagedResultDto<T> WithCollectionLinks<T>(string resourceName, PagedResultDto<T> page, Func<int,int,string> pageUrlFactory) where T: class;
}

public class LinkBuilder : ILinkBuilder
{
    public ResourceDto<T> WithItemLinks<T>(string resourceName, Guid id, ResourceDto<T> resource) where T : class
    {
        // Minimal HATEOAS: self e collection
        var basePath = $"/api/v1/{resourceName}";
        resource.Links.Add(new LinkDto("self", $"{basePath}/{id}", "GET"));
        resource.Links.Add(new LinkDto("collection", basePath, "GET"));
        return resource;
    }

    public PagedResultDto<T> WithCollectionLinks<T>(string resourceName, PagedResultDto<T> page, Func<int,int,string> pageUrlFactory) where T: class
    {
        var basePath = $"/api/v1/{resourceName}";
        page.Links.Add(new LinkDto("self", pageUrlFactory(page.Page, page.PageSize), "GET"));
        if (page.HasPrev)
            page.Links.Add(new LinkDto("prev", pageUrlFactory(page.Page - 1, page.PageSize), "GET"));
        if (page.HasNext)
            page.Links.Add(new LinkDto("next", pageUrlFactory(page.Page + 1, page.PageSize), "GET"));
        return page;
    }
}