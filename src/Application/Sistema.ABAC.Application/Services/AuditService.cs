using AutoMapper;
using Microsoft.Extensions.Logging;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;

namespace Sistema.ABAC.Application.Services;

/// <summary>
/// Implementación del servicio de auditoría para evaluaciones ABAC.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AuditService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AccessLogDto> LogAccessEvaluationAsync(
        Guid userId,
        Guid? resourceId,
        Guid? actionId,
        string result,
        string? reason = null,
        Guid? policyId = null,
        string? context = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            result = "Error";
        }

        var accessLog = new AccessLog
        {
            UserId = userId,
            ResourceId = resourceId,
            ActionId = actionId,
            PolicyId = policyId,
            Result = result.Trim(),
            Reason = reason,
            Context = context,
            IpAddress = ipAddress
        };

        await _unitOfWork.AccessLogs.AddAsync(accessLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Log de auditoría registrado: UserId={UserId}, ResourceId={ResourceId}, ActionId={ActionId}, Result={Result}",
            userId,
            resourceId,
            actionId,
            accessLog.Result);

        return _mapper.Map<AccessLogDto>(accessLog);
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<AccessLogDto>> GetLogsAsync(
        AccessLogFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        filter ??= new AccessLogFilterDto();

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        if (filter.PageSize < 1)
        {
            filter.PageSize = 50;
        }

        if (filter.PageSize > 200)
        {
            filter.PageSize = 200;
        }

        var skip = (filter.Page - 1) * filter.PageSize;

        var logs = await _unitOfWork.AccessLogs.GetWithFiltersAsync(
            userId: filter.UserId,
            resourceId: filter.ResourceId,
            actionId: filter.ActionId,
            result: filter.Result,
            fromDate: filter.FromDate,
            toDate: filter.ToDate,
            sortBy: filter.SortBy,
            sortDescending: filter.SortDescending,
            skip: skip,
            take: filter.PageSize,
            cancellationToken: cancellationToken);

        var totalCount = await _unitOfWork.AccessLogs.CountAsync(log =>
            (!filter.UserId.HasValue || log.UserId == filter.UserId.Value) &&
            (!filter.ResourceId.HasValue || log.ResourceId == filter.ResourceId.Value) &&
            (!filter.ActionId.HasValue || log.ActionId == filter.ActionId.Value) &&
            (string.IsNullOrWhiteSpace(filter.Result) || log.Result == filter.Result) &&
            (!filter.FromDate.HasValue || log.CreatedAt >= filter.FromDate.Value) &&
            (!filter.ToDate.HasValue || log.CreatedAt <= filter.ToDate.Value),
            cancellationToken);

        return new PagedResultDto<AccessLogDto>
        {
            Items = _mapper.Map<List<AccessLogDto>>(logs),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    /// <inheritdoc />
    public async Task<AccessLogStatisticsDto> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var statistics = await _unitOfWork.AccessLogs.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return _mapper.Map<AccessLogStatisticsDto>(statistics);
    }
}