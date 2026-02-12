using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Infrastructure.Persistence;

namespace Sistema.ABAC.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para recursos.
/// Maneja los recursos protegidos por el sistema ABAC.
/// </summary>
public class ResourceRepository : Repository<Resource>, IResourceRepository
{
    public ResourceRepository(AbacDbContext context) : base(context)
    {
    }

    public async Task<Resource?> GetWithAttributesAsync(
        Guid resourceId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.ResourceAttributes)
                .ThenInclude(ra => ra.Attribute)
            .FirstOrDefaultAsync(r => r.Id == resourceId, cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetByTypeAsync(
        string type, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> GetByAttributeAsync(
        string attributeKey, 
        string attributeValue, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.ResourceAttributes)
                .ThenInclude(ra => ra.Attribute)
            .Where(r => r.ResourceAttributes.Any(ra => 
                ra.Attribute.Key == attributeKey && 
                ra.Value == attributeValue))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Resource>> SearchByNameAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => EF.Functions.Like(r.Name, $"%{searchTerm}%"))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccessLog>> GetAccessLogsAsync(
        Guid resourceId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.AccessLogs
            .Include(al => al.User)
            .Include(al => al.Action)
            .Where(al => al.ResourceId == resourceId)
            .OrderByDescending(al => al.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
