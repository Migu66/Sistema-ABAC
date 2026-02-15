using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Infrastructure.Persistence;

namespace Sistema.ABAC.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para logs de auditoría.
/// Maneja el registro y consulta de accesos al sistema.
/// </summary>
public class AccessLogRepository : Repository<AccessLog>, IAccessLogRepository
{
    public AccessLogRepository(AbacDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AccessLog>> GetByUserAsync(
        Guid userId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(al => al.Resource)
            .Include(al => al.Action)
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccessLog>> GetByResourceAsync(
        Guid resourceId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(al => al.User)
            .Include(al => al.Action)
            .Where(al => al.ResourceId == resourceId)
            .OrderByDescending(al => al.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccessLog>> GetByActionAsync(
        Guid actionId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(al => al.User)
            .Include(al => al.Resource)
            .Where(al => al.ActionId == actionId)
            .OrderByDescending(al => al.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccessLog>> GetByResultAsync(
        string result, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(al => al.User)
            .Include(al => al.Resource)
            .Include(al => al.Action)
            .Where(al => al.Result == result)
            .OrderByDescending(al => al.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccessLog>> GetByDateRangeAsync(
        DateTime fromDate, 
        DateTime toDate, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(al => al.User)
            .Include(al => al.Resource)
            .Include(al => al.Action)
            .Where(al => al.CreatedAt >= fromDate && al.CreatedAt <= toDate)
            .OrderByDescending(al => al.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccessLog>> GetWithFiltersAsync(
        Guid? userId = null,
        Guid? resourceId = null,
        Guid? actionId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string sortBy = "CreatedAt",
        bool sortDescending = true,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(al => al.User)
            .Include(al => al.Resource)
            .Include(al => al.Action)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(al => al.UserId == userId.Value);

        if (resourceId.HasValue)
            query = query.Where(al => al.ResourceId == resourceId.Value);

        if (actionId.HasValue)
            query = query.Where(al => al.ActionId == actionId.Value);

        if (!string.IsNullOrWhiteSpace(result))
            query = query.Where(al => al.Result == result);

        if (fromDate.HasValue)
            query = query.Where(al => al.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.CreatedAt <= toDate.Value);

        query = sortBy?.ToLowerInvariant() switch
        {
            "result" => sortDescending
                ? query.OrderByDescending(al => al.Result)
                : query.OrderBy(al => al.Result),
            "userid" => sortDescending
                ? query.OrderByDescending(al => al.UserId)
                : query.OrderBy(al => al.UserId),
            "resourceid" => sortDescending
                ? query.OrderByDescending(al => al.ResourceId)
                : query.OrderBy(al => al.ResourceId),
            "actionid" => sortDescending
                ? query.OrderByDescending(al => al.ActionId)
                : query.OrderBy(al => al.ActionId),
            _ => sortDescending
                ? query.OrderByDescending(al => al.CreatedAt)
                : query.OrderBy(al => al.CreatedAt)
        };

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        Guid? userId = null,
        Guid? resourceId = null,
        Guid? actionId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (userId.HasValue)
            query = query.Where(al => al.UserId == userId.Value);

        if (resourceId.HasValue)
            query = query.Where(al => al.ResourceId == resourceId.Value);

        if (actionId.HasValue)
            query = query.Where(al => al.ActionId == actionId.Value);

        if (!string.IsNullOrWhiteSpace(result))
            query = query.Where(al => al.Result == result);

        if (fromDate.HasValue)
            query = query.Where(al => al.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.CreatedAt <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<AccessLogStatistics> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(al => al.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.CreatedAt <= toDate.Value);

        var totalAttempts = await query.CountAsync(cancellationToken);
        var permittedAccess = await query.CountAsync(al => al.Result == "Permit", cancellationToken);
        var deniedAccess = await query.CountAsync(al => al.Result == "Deny", cancellationToken);
        var errors = await query.CountAsync(al => al.Result == "Error", cancellationToken);

        return new AccessLogStatistics
        {
            TotalAttempts = totalAttempts,
            PermittedAccess = permittedAccess,
            DeniedAccess = deniedAccess,
            Errors = errors
        };
    }

    public async Task<Dictionary<string, int>> GetAccessStatisticsByResultAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(al => al.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.CreatedAt <= toDate.Value);

        return await query
            .GroupBy(al => al.Result)
            .Select(g => new { Result = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Result, x => x.Count, cancellationToken);
    }

    public async Task<IEnumerable<(Guid ResourceId, int AccessCount)>> GetMostAccessedResourcesAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int top = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(al => al.ResourceId != null);

        if (fromDate.HasValue)
            query = query.Where(al => al.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.CreatedAt <= toDate.Value);

        var result = await query
            .GroupBy(al => al.ResourceId!.Value)
            .Select(g => new { ResourceId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(cancellationToken);

        return result.Select(x => (x.ResourceId, x.Count));
    }

    public async Task<IEnumerable<(Guid UserId, int AccessCount)>> GetMostActiveUsersAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int top = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(al => al.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.CreatedAt <= toDate.Value);

        var result = await query
            .GroupBy(al => al.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(cancellationToken);

        return result.Select(x => (x.UserId, x.Count));
    }

    public async Task<IEnumerable<(Guid PolicyId, int DenialCount)>> GetDenialsByPolicyAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(al => al.Result == "Deny" && al.PolicyId != null);

        if (fromDate.HasValue)
            query = query.Where(al => al.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.CreatedAt <= toDate.Value);

        var result = await query
            .GroupBy(al => al.PolicyId!.Value)
            .Select(g => new { PolicyId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return result.Select(x => (x.PolicyId, x.Count));
    }

    public async Task<IEnumerable<AccessLog>> GetRecentAccessLogsAsync(
        int count = 100, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(al => al.User)
            .Include(al => al.Resource)
            .Include(al => al.Action)
            .OrderByDescending(al => al.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}

