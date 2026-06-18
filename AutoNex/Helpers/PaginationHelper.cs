using AutoNex.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoNex.Helpers;

public static class PaginationHelper
{
    public static async Task<PagedResponse<T>> ToPagedAsync<T>(
        this IQueryable<T> query, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var p = Math.Max(page ?? 1, 1);
        var ps = Math.Clamp(pageSize ?? 20, 1, 100);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((p - 1) * ps)
            .Take(ps)
            .ToListAsync(cancellationToken);

        return new PagedResponse<T>
        {
            Items = items,
            Page = p,
            PageSize = ps,
            TotalCount = totalCount
        };
    }

    public static async Task<PagedResponse<TResponse>> ToPagedResponseAsync<TEntity, TResponse>(
        this IQueryable<TEntity> query, int? page, int? pageSize, Func<TEntity, TResponse> mapper, CancellationToken cancellationToken = default)
    {
        var p = Math.Max(page ?? 1, 1);
        var ps = Math.Clamp(pageSize ?? 20, 1, 100);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((p - 1) * ps)
            .Take(ps)
            .ToListAsync(cancellationToken);

        return new PagedResponse<TResponse>
        {
            Items = items.Select(mapper).ToList(),
            Page = p,
            PageSize = ps,
            TotalCount = totalCount
        };
    }
}
