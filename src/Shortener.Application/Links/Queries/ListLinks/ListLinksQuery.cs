namespace Shortener.Application.Links.Queries.ListLinks;

public sealed record ListLinksQuery(Guid TenantId, int PageSize = 20, Guid? AfterId = null);
