using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.API.Controllers;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.Tests.API.Controllers;

public class AuditControllerTests
{
    private readonly Mock<IAuditService> _serviceMock;
    private readonly AuditController _sut;

    public AuditControllerTests()
    {
        _serviceMock = new Mock<IAuditService>();
        _sut = new AuditController(_serviceMock.Object, NullLogger<AuditController>.Instance);
    }

    [Fact]
    public async Task GetLogs_ReturnsOk_WithPagedResult()
    {
        var filter = new AccessLogFilterDto { Page = 1, PageSize = 10 };
        var paged = new PagedResultDto<AccessLogDto>
        {
            Items = new List<AccessLogDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _serviceMock.Setup(s => s.GetLogsAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetLogs(filter);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(paged);
    }

    [Fact]
    public async Task GetStatistics_ReturnsOk()
    {
        var from = DateTime.UtcNow.AddDays(-30);
        var to = DateTime.UtcNow;
        var stats = new AccessLogStatisticsDto
        {
            TotalAttempts = 100,
            PermittedAccess = 80,
            DeniedAccess = 20,
            Errors = 0
        };

        _serviceMock.Setup(s => s.GetStatisticsAsync(from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var result = await _sut.GetStatistics(from, to);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(stats);
    }

    [Fact]
    public async Task GetStatistics_WithNullDates_ReturnsOk()
    {
        var stats = new AccessLogStatisticsDto();

        _serviceMock.Setup(s => s.GetStatisticsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var result = await _sut.GetStatistics(null, null);

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}
