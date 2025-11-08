using System.Text.Json.Serialization;

namespace AuthService.Api.Models;

public sealed class PagedResult<T>
{
    [JsonPropertyName("items")] public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    [JsonPropertyName("total")] public int Total { get; init; }
    [JsonPropertyName("page")] public int Page { get; init; }
    [JsonPropertyName("pageSize")] public int PageSize { get; init; }

    public static PagedResult<T> Create(IEnumerable<T> items, int total, int page, int pageSize)
        => new() { Items = items.ToList(), Total = total, Page = page, PageSize = pageSize };
}
