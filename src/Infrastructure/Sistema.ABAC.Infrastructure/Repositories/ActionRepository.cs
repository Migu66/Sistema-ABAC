using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Infrastructure.Persistence;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para acciones.
/// Maneja las acciones que se pueden realizar sobre recursos.
/// </summary>
public class ActionRepository : Repository<ActionEntity>, IActionRepository
{
    public ActionRepository(AbacDbContext context) : base(context)
    {
    }

    public async Task<ActionEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Code == code, cancellationToken);
    }

    public async Task<ActionEntity?> GetWithPoliciesAsync(
        Guid actionId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.PolicyActions)
                .ThenInclude(pa => pa.Policy)
            .FirstOrDefaultAsync(a => a.Id == actionId, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(
        string code, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(a => a.Code == code);
        
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActionEntity>> GetActionsWithActivePoliciesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.PolicyActions)
                .ThenInclude(pa => pa.Policy)
            .Where(a => a.PolicyActions.Any(pa => pa.Policy.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccessLog>> GetAccessLogsAsync(
        Guid actionId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.AccessLogs
            .Include(al => al.User)
            .Include(al => al.Resource)
            .Where(al => al.ActionId == actionId)
            .OrderByDescending(al => al.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
