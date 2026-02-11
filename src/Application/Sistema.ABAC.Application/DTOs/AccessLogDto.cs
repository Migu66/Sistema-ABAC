namespace Sistema.ABAC.Application.DTOs;

/// <summary>
/// DTO para logs de auditoría de accesos.
/// </summary>
public class AccessLogDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string? UserName { get; set; }

    public Guid? ResourceId { get; set; }
    public string? ResourceName { get; set; }

    public Guid? ActionId { get; set; }
    public string? ActionName { get; set; }

    public Guid? PolicyId { get; set; }
    public string? PolicyName { get; set; }

    public string Result { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Context { get; set; }
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para filtros de búsqueda de logs de acceso.
/// </summary>
public class AccessLogFilterDto
{
    public Guid? UserId { get; set; }
    public Guid? ResourceId { get; set; }
    public Guid? ActionId { get; set; }
    public string? Result { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// DTO para estadísticas de acceso.
/// </summary>
public class AccessLogStatisticsDto
{
    public int TotalAttempts { get; set; }
    public int PermittedAccess { get; set; }
    public int DeniedAccess { get; set; }
    public int Errors { get; set; }
    public double PermitRate { get; set; }
    public double DenyRate { get; set; }
    public double ErrorRate { get; set; }
}
