using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using Sistema.ABAC.Infrastructure.Persistence;

namespace Sistema.ABAC.Infrastructure.Repositories;

/// <summary>
/// Implementación del patrón Unit of Work.
/// Coordina el trabajo entre múltiples repositorios y gestiona transacciones.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AbacDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    // Repositorios lazy-initialized
    private IUserRepository? _users;
    private IAttributeRepository? _attributes;
    private IResourceRepository? _resources;
    private IActionRepository? _actions;
    private IPolicyRepository? _policies;
    private IAccessLogRepository? _accessLogs;
    private IRepository<UserAttribute>? _userAttributes;
    private IRepository<ResourceAttribute>? _resourceAttributes;
    private IRepository<PolicyCondition>? _policyConditions;
    private IRepository<PolicyAction>? _policyActions;

    public UnitOfWork(
        AbacDbContext context,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public IUserRepository Users
    {
        get
        {
            _users ??= new UserRepository(_context);
            return _users;
        }
    }

    public IAttributeRepository Attributes
    {
        get
        {
            _attributes ??= new AttributeRepository(_context);
            return _attributes;
        }
    }

    public IResourceRepository Resources
    {
        get
        {
            _resources ??= new ResourceRepository(_context);
            return _resources;
        }
    }

    public IActionRepository Actions
    {
        get
        {
            _actions ??= new ActionRepository(_context);
            return _actions;
        }
    }

    public IPolicyRepository Policies
    {
        get
        {
            _policies ??= new PolicyRepository(_context);
            return _policies;
        }
    }

    public IAccessLogRepository AccessLogs
    {
        get
        {
            _accessLogs ??= new AccessLogRepository(_context);
            return _accessLogs;
        }
    }

    public IRepository<UserAttribute> UserAttributes
    {
        get
        {
            _userAttributes ??= new Repository<UserAttribute>(_context);
            return _userAttributes;
        }
    }

    public IRepository<ResourceAttribute> ResourceAttributes
    {
        get
        {
            _resourceAttributes ??= new Repository<ResourceAttribute>(_context);
            return _resourceAttributes;
        }
    }

    public IRepository<PolicyCondition> PolicyConditions
    {
        get
        {
            _policyConditions ??= new Repository<PolicyCondition>(_context);
            return _policyConditions;
        }
    }

    public IRepository<PolicyAction> PolicyActions
    {
        get
        {
            _policyActions ??= new Repository<PolicyAction>(_context);
            return _policyActions;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new DbTransaction(transaction);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}

/// <summary>
/// Wrapper para transacciones de base de datos.
/// </summary>
public class DbTransaction : IDbTransaction
{
    private readonly IDbContextTransaction _transaction;

    public DbTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        _transaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}
