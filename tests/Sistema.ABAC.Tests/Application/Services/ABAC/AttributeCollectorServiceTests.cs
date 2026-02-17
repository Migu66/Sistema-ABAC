using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Domain.Interfaces;
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;

namespace Sistema.ABAC.Tests.Application.Services.ABAC;

public class AttributeCollectorServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IResourceRepository> _resourceRepoMock;
    private readonly AttributeCollectorService _sut;

    public AttributeCollectorServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _resourceRepoMock = new Mock<IResourceRepository>();

        _unitOfWorkMock.SetupGet(u => u.Users).Returns(_userRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Resources).Returns(_resourceRepoMock.Object);

        _sut = new AttributeCollectorService(
            _unitOfWorkMock.Object,
            NullLogger<AttributeCollectorService>.Instance);
    }

    #region CollectSubjectAttributesAsync

    [Fact]
    public async Task CollectSubjectAttributesAsync_ValidUser_ReturnsAttributes()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, FirstName = "Test", LastName = "User", IsDeleted = false };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var activeAttributes = new List<UserAttribute>
        {
            new()
            {
                UserId = userId,
                Attribute = new AttributeEntity { Key = "level", Type = AttributeType.Number },
                Value = "5"
            }
        };

        _userRepoMock.Setup(r => r.GetActiveAttributesAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeAttributes);

        var result = await _sut.CollectSubjectAttributesAsync(userId);

        result.Should().ContainKey("level");
        result["level"].Should().Be(5m); // Number type converts to decimal
    }

    [Fact]
    public async Task CollectSubjectAttributesAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _sut.CollectSubjectAttributesAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CollectSubjectAttributesAsync_DeletedUser_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, IsDeleted = true });

        var act = async () => await _sut.CollectSubjectAttributesAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CollectSubjectAttributesAsync_BooleanAttribute_ConvertedCorrectly()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, IsDeleted = false });
        _userRepoMock.Setup(r => r.GetActiveAttributesAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAttribute>
            {
                new() { Attribute = new AttributeEntity { Key = "active", Type = AttributeType.Boolean }, Value = "true" }
            });

        var result = await _sut.CollectSubjectAttributesAsync(userId);

        result["active"].Should().Be(true);
    }

    [Fact]
    public async Task CollectSubjectAttributesAsync_DateTimeAttribute_ConvertedCorrectly()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, IsDeleted = false });
        _userRepoMock.Setup(r => r.GetActiveAttributesAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAttribute>
            {
                new() { Attribute = new AttributeEntity { Key = "expiry", Type = AttributeType.DateTime }, Value = "2025-01-15" }
            });

        var result = await _sut.CollectSubjectAttributesAsync(userId);

        result["expiry"].Should().BeOfType<DateTime>();
    }

    [Fact]
    public async Task CollectSubjectAttributesAsync_StringAttribute_RemainsString()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, IsDeleted = false });
        _userRepoMock.Setup(r => r.GetActiveAttributesAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAttribute>
            {
                new() { Attribute = new AttributeEntity { Key = "dept", Type = AttributeType.String }, Value = "Engineering" }
            });

        var result = await _sut.CollectSubjectAttributesAsync(userId);

        result["dept"].Should().Be("Engineering");
    }

    [Fact]
    public async Task CollectSubjectAttributesAsync_NullAttributeKey_SkipsEntry()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, IsDeleted = false });
        _userRepoMock.Setup(r => r.GetActiveAttributesAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAttribute>
            {
                new() { Attribute = new AttributeEntity { Key = null!, Type = AttributeType.String }, Value = "val" },
                new() { Attribute = null, Value = "val" }
            });

        var result = await _sut.CollectSubjectAttributesAsync(userId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CollectSubjectAttributesAsync_NullValue_ReturnsAsIs()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, IsDeleted = false });
        _userRepoMock.Setup(r => r.GetActiveAttributesAsync(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAttribute>
            {
                new() { Attribute = new AttributeEntity { Key = "key", Type = AttributeType.Number }, Value = null }
            });

        var result = await _sut.CollectSubjectAttributesAsync(userId);

        result["key"].Should().BeNull();
    }

    #endregion

    #region CollectResourceAttributesAsync

    [Fact]
    public async Task CollectResourceAttributesAsync_ValidResource_ReturnsAttributes()
    {
        var resourceId = Guid.NewGuid();
        var resource = new Resource
        {
            Id = resourceId,
            Name = "API",
            Type = "Service",
            IsDeleted = false,
            ResourceAttributes = new List<ResourceAttribute>
            {
                new()
                {
                    Attribute = new AttributeEntity { Key = "level", Type = AttributeType.Number },
                    Value = "3"
                }
            }
        };

        _resourceRepoMock.Setup(r => r.GetWithAttributesAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        var result = await _sut.CollectResourceAttributesAsync(resourceId);

        result.Should().ContainKey("level");
        result["level"].Should().Be(3m);
    }

    [Fact]
    public async Task CollectResourceAttributesAsync_NotFound_ThrowsNotFoundException()
    {
        _resourceRepoMock.Setup(r => r.GetWithAttributesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);

        var act = async () => await _sut.CollectResourceAttributesAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CollectResourceAttributesAsync_DeletedResource_ThrowsNotFoundException()
    {
        var resourceId = Guid.NewGuid();
        _resourceRepoMock.Setup(r => r.GetWithAttributesAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Resource
            {
                Id = resourceId,
                IsDeleted = true,
                ResourceAttributes = new List<ResourceAttribute>()
            });

        var act = async () => await _sut.CollectResourceAttributesAsync(resourceId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region CollectEnvironmentAttributesAsync

    [Fact]
    public async Task CollectEnvironmentAttributesAsync_NoContext_ReturnsDefaultAttributes()
    {
        var result = await _sut.CollectEnvironmentAttributesAsync();

        result.Should().ContainKey("currentUtcDateTime");
        result.Should().ContainKey("currentLocalDateTime");
        result.Should().ContainKey("currentDate");
        result.Should().ContainKey("currentHour");
        result.Should().ContainKey("dayOfWeek");
        result.Should().ContainKey("dayOfWeekNumber");
        result.Should().ContainKey("ipAddress");
        result.Should().ContainKey("location");
    }

    [Fact]
    public async Task CollectEnvironmentAttributesAsync_WithContext_MergesAttributes()
    {
        var context = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["customKey"] = "customValue"
        };

        var result = await _sut.CollectEnvironmentAttributesAsync(context);

        result.Should().ContainKey("customKey");
        result["customKey"].Should().Be("customValue");
    }

    [Fact]
    public async Task CollectEnvironmentAttributesAsync_WithIp_SetsIpAddress()
    {
        var context = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["ip"] = "192.168.1.1"
        };

        var result = await _sut.CollectEnvironmentAttributesAsync(context);

        result["ipAddress"].Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task CollectEnvironmentAttributesAsync_WithGeoLocation_SetsLocation()
    {
        var context = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["geoLocation"] = "US"
        };

        var result = await _sut.CollectEnvironmentAttributesAsync(context);

        result["location"].Should().Be("US");
    }

    [Fact]
    public async Task CollectEnvironmentAttributesAsync_CancellationRequested_Throws()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _sut.CollectEnvironmentAttributesAsync(cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
