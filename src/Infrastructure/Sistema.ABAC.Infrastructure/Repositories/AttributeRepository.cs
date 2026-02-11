using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Infrastructure.Persistence;
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;

namespace Sistema.ABAC.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para atributos.
/// Maneja las definiciones de atributos del sistema ABAC.
/// </summary>
public class AttributeRepository : Repository<AttributeEntity>, IAttributeRepository
{
    public AttributeRepository(AbacDbContext context) : base(context)
    {
    }

    public async Task<AttributeEntity?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Key == key, cancellationToken);
    }

    public async Task<AttributeEntity?> GetWithUserAttributesAsync(
        Guid attributeId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.UserAttributes)
                .ThenInclude(ua => ua.User)
            .FirstOrDefaultAsync(a => a.Id == attributeId, cancellationToken);
    }

    public async Task<AttributeEntity?> GetWithResourceAttributesAsync(
        Guid attributeId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.ResourceAttributes)
                .ThenInclude(ra => ra.Resource)
            .FirstOrDefaultAsync(a => a.Id == attributeId, cancellationToken);
    }

    public async Task<bool> KeyExistsAsync(
        string key, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(a => a.Key == key);
        
        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<AttributeEntity>> GetAttributesUsedInPoliciesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => _context.PolicyConditions
                .Any(pc => pc.AttributeKey == a.Key))
            .ToListAsync(cancellationToken);
    }
}
