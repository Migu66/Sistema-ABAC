using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using AutoMapper;
using Sistema.ABAC.Application.Mappings;
using System.Linq.Expressions;

namespace Sistema.ABAC.Tests.Application.Services;

public class AuditServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAccessLogRepository> _logRepoMock;
    private readonly IMapper _mapper;
    private readonly AuditService _sut;

    public AuditServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _logRepoMock = new Mock<IAccessLogRepository>();

        _unitOfWorkMock.SetupGet(u => u.AccessLogs).Returns(_logRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new AuditService(
            _unitOfWorkMock.Object,
            _mapper,
            NullLogger<AuditService>.Instance);
    }

    #region LogAccessEvaluationAsync

    [Fact]
    public async Task LogAccessEvaluationAsync_WithValidResult_CreatesLog()
    {
        var userId = Guid.NewGuid();
        _logRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AccessLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccessLog log, CancellationToken _) => log);

        var result = await _sut.LogAccessEvaluationAsync(userId, Guid.NewGuid(), Guid.NewGuid(), "Permit", "Allowed", null, null, "127.0.0.1");

        result.Should().NotBeNull();
        result.Result.Should().Be("Permit");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogAccessEvaluationAsync_WhenResultIsNullOrWhitespace_DefaultsToError()
    {
        var userId = Guid.NewGuid();
        _logRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AccessLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccessLog log, CancellationToken _) => log);

        var result = await _sut.LogAccessEvaluationAsync(userId, null, null, "  ");

        result.Result.Should().Be("Error");
    }

    [Fact]
    public async Task LogAccessEvaluationAsync_TrimsResult()
    {
        var userId = Guid.NewGuid();
        _logRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AccessLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccessLog log, CancellationToken _) => log);

        var result = await _sut.LogAccessEvaluationAsync(userId, null, null, "  Permit  ");

        result.Result.Should().Be("Permit");
    }

    #endregion

    #region GetLogsAsync

    [Fact]
    public async Task GetLogsAsync_WithDefaultFilter_ReturnsPagedResults()
    {
        var logs = new List<AccessLog>
        {
            new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Result = "Permit", CreatedAt = DateTime.UtcNow }
        };

        _logRepoMock
            .Setup(r => r.GetWithFiltersAsync(null, null, null, null, null, null, "CreatedAt", true, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        _logRepoMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<AccessLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.GetLogsAsync(new AccessLogFilterDto());

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetLogsAsync_WithNullFilter_UsesDefaults()
    {
        _logRepoMock
            .Setup(r => r.GetWithFiltersAsync(null, null, null, null, null, null, "CreatedAt", true, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AccessLog>());

        _logRepoMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<AccessLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _sut.GetLogsAsync(null!);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLogsAsync_NormalizesPageBelowOne()
    {
        _logRepoMock
            .Setup(r => r.GetWithFiltersAsync(null, null, null, null, null, null, "CreatedAt", true, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AccessLog>());

        _logRepoMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<AccessLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var filter = new AccessLogFilterDto { Page = -1, PageSize = 0 };
        var result = await _sut.GetLogsAsync(filter);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetLogsAsync_CapsPageSizeAt200()
    {
        _logRepoMock
            .Setup(r => r.GetWithFiltersAsync(null, null, null, null, null, null, "CreatedAt", true, 0, 200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AccessLog>());

        _logRepoMock
            .Setup(r => r.CountAsync(It.IsAny<Expression<Func<AccessLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var filter = new AccessLogFilterDto { PageSize = 500 };
        var result = await _sut.GetLogsAsync(filter);

        result.PageSize.Should().Be(200);
    }

    #endregion

    #region GetStatisticsAsync

    [Fact]
    public async Task GetStatisticsAsync_ReturnsMappedStatistics()
    {
        var stats = new AccessLogStatistics
        {
            TotalAttempts = 100,
            PermittedAccess = 80,
            DeniedAccess = 15,
            Errors = 5
        };

        _logRepoMock
            .Setup(r => r.GetStatisticsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var result = await _sut.GetStatisticsAsync();

        result.TotalAttempts.Should().Be(100);
        result.PermittedAccess.Should().Be(80);
        result.DeniedAccess.Should().Be(15);
        result.Errors.Should().Be(5);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithDateRange_PassesDatesToRepo()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        _logRepoMock
            .Setup(r => r.GetStatisticsAsync(from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessLogStatistics());

        await _sut.GetStatisticsAsync(from, to);

        _logRepoMock.Verify(r => r.GetStatisticsAsync(from, to, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
