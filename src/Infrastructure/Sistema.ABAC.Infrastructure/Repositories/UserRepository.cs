using Microsoft.EntityFrameworkCore;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Infrastructure.Persistence;

namespace Sistema.ABAC.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para usuarios.
/// Maneja operaciones relacionadas con usuarios y sus atributos ABAC.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AbacDbContext _context;

    public UserRepository(AbacDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Remove(User user)
    {
        // Soft delete: marcar como eliminado sin borrar físicamente
        user.IsDeleted = true;
        Update(user);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<User?> GetWithAttributesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserAttributes)
                .ThenInclude(ua => ua.Attribute)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<IEnumerable<UserAttribute>> GetActiveAttributesAsync(
        Guid userId, 
        DateTime? evaluationDate = null, 
        CancellationToken cancellationToken = default)
    {
        var date = evaluationDate ?? DateTime.UtcNow;

        return await _context.UserAttributes
            .Include(ua => ua.Attribute)
            .Where(ua => ua.UserId == userId &&
                        (ua.ValidFrom == null || ua.ValidFrom <= date) &&
                        (ua.ValidTo == null || ua.ValidTo >= date))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByAttributeAsync(
        string attributeKey, 
        string attributeValue, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserAttributes)
                .ThenInclude(ua => ua.Attribute)
            .Where(u => u.UserAttributes.Any(ua => 
                ua.Attribute.Key == attributeKey && 
                ua.Value == attributeValue))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AccessLog>> GetAccessLogsAsync(
        Guid userId, 
        int skip = 0, 
        int take = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.AccessLogs
            .Include(al => al.Resource)
            .Include(al => al.Action)
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
