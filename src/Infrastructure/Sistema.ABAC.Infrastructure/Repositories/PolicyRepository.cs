using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Infrastructure.Persistence;

namespace Sistema.ABAC.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para políticas ABAC.
/// Este es uno de los repositorios más importantes del sistema.
/// </summary>
public class PolicyRepository : Repository<Policy>, IPolicyRepository
{
    public PolicyRepository(AbacDbContext context) : base(context)
    {
    }

    public async Task<Policy?> GetWithDetailsAsync(
        Guid policyId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Conditions)
            .Include(p => p.PolicyActions)
                .ThenInclude(pa => pa.Action)
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetActivePoliciesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Conditions)
            .Include(p => p.PolicyActions)
                .ThenInclude(pa => pa.Action)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetActivePoliciesForActionAsync(
        Guid actionId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Conditions)
            .Include(p => p.PolicyActions)
                .ThenInclude(pa => pa.Action)
            .Where(p => p.IsActive && 
                       p.PolicyActions.Any(pa => pa.ActionId == actionId))
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetPoliciesByPriorityAsync(
        Guid? actionId = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.Conditions)
            .Include(p => p.PolicyActions)
                .ThenInclude(pa => pa.Action)
            .Where(p => p.IsActive);

        if (actionId.HasValue)
        {
            query = query.Where(p => p.PolicyActions.Any(pa => pa.ActionId == actionId.Value));
        }

        return await query
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetByEffectAsync(
        PolicyEffect effect, 
        bool activeOnly = true, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Effect == effect);

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Policy>> SearchByNameAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%"))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetPoliciesUsingAttributeAsync(
        string attributeKey, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Conditions)
            .Where(p => p.Conditions.Any(pc => pc.AttributeKey == attributeKey))
            .ToListAsync(cancellationToken);
    }

    public async Task<(int Active, int Inactive)> GetPolicyStatusCountAsync(
        CancellationToken cancellationToken = default)
    {
        var active = await _dbSet.CountAsync(p => p.IsActive, cancellationToken);
        var inactive = await _dbSet.CountAsync(p => !p.IsActive, cancellationToken);
        
        return (active, inactive);
    }

    public async Task<int> GetPolicyApplicationCountAsync(
        Guid policyId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default)
    {
        // Nota: Esto funcionará cuando tengamos AccessLogs con referencia a políticas aplicadas
        // Por ahora retornamos 0 como placeholder
        // TODO: Implementar después de añadir campo AppliedPolicies en AccessLog
        return await Task.FromResult(0);
    }
}
