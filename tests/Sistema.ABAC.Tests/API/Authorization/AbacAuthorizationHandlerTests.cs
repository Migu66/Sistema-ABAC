using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.API.Authorization;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Interfaces;
using AbacAuthResult = Sistema.ABAC.Application.Services.ABAC.AuthorizationResult;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Tests.API.Authorization;

public class AbacAuthorizationHandlerTests
{
    private readonly Mock<IAccessControlService> _accessControlServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IActionRepository> _actionRepoMock;
    private readonly AbacAuthorizationHandler _sut;

    public AbacAuthorizationHandlerTests()
    {
        _accessControlServiceMock = new Mock<IAccessControlService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _actionRepoMock = new Mock<IActionRepository>();
        _unitOfWorkMock.Setup(u => u.Actions).Returns(_actionRepoMock.Object);

        _sut = new AbacAuthorizationHandler(
            _accessControlServiceMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<AbacAuthorizationHandler>.Instance);
    }

    #region Helpers

    private static AuthorizationHandlerContext CreateContext(
        AbacRequirement requirement,
        ClaimsPrincipal user,
        object? resource = null)
    {
        var requirements = new[] { requirement };
        return new AuthorizationHandlerContext(requirements, user, resource);
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(Guid? userId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser")
        };

        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestScheme");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUnauthenticatedUser()
    {
        var identity = new ClaimsIdentity(); // no auth type = unauthenticated
        return new ClaimsPrincipal(identity);
    }

    private static HttpContext CreateHttpContext(
        string method = "GET",
        Dictionary<string, object>? routeValues = null,
        string? queryResourceId = null,
        string? queryActionId = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;

        if (routeValues != null)
        {
            foreach (var kv in routeValues)
            {
                httpContext.Request.RouteValues[kv.Key] = kv.Value;
            }
        }

        if (queryResourceId != null)
        {
            httpContext.Request.QueryString = new QueryString($"?resourceId={queryResourceId}" +
                (queryActionId != null ? $"&actionId={queryActionId}" : ""));
        }
        else if (queryActionId != null)
        {
            httpContext.Request.QueryString = new QueryString($"?actionId={queryActionId}");
        }

        return httpContext;
    }

    #endregion

    [Fact]
    public async Task HandleRequirementAsync_WhenUnauthenticated_DoesNotSucceed()
    {
        var user = CreateUnauthenticatedUser();
        var requirement = new AbacRequirement();
        var context = CreateContext(requirement, user);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenNoUserId_DoesNotSucceed()
    {
        var user = CreateAuthenticatedUser(); // no userId claim
        var requirement = new AbacRequirement();
        var context = CreateContext(requirement, user);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenNoHttpContext_DoesNotSucceed()
    {
        var userId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(userId);
        var requirement = new AbacRequirement(Guid.NewGuid(), Guid.NewGuid());
        // resource = null, so no httpContext
        var context = CreateContext(requirement, user, null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenPermit_Succeeds()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(userId);

        var httpContext = CreateHttpContext("GET",
            new Dictionary<string, object> { { "resourceId", resourceId.ToString() } },
            queryActionId: actionId.ToString());

        var requirement = new AbacRequirement(resourceId, actionId);
        var context = CreateContext(requirement, user, httpContext);

        _accessControlServiceMock.Setup(s => s.CheckAccessAsync(
                userId, resourceId, actionId,
                It.IsAny<Dictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AbacAuthResult
            {
                Decision = AuthorizationDecision.Permit
            });

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenDeny_DoesNotSucceed()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(userId);

        var httpContext = CreateHttpContext("GET");
        httpContext.Request.RouteValues["id"] = resourceId.ToString();

        var requirement = new AbacRequirement(resourceId, actionId);
        var context = CreateContext(requirement, user, httpContext);

        _accessControlServiceMock.Setup(s => s.CheckAccessAsync(
                userId, resourceId, actionId,
                It.IsAny<Dictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AbacAuthResult
            {
                Decision = AuthorizationDecision.Deny
            });

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ResolvesResourceIdFromRouteId()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(userId);

        var httpContext = CreateHttpContext("GET");
        httpContext.Request.RouteValues["id"] = resourceId.ToString();

        var requirement = new AbacRequirement(null, actionId);
        var context = CreateContext(requirement, user, httpContext);

        _accessControlServiceMock.Setup(s => s.CheckAccessAsync(
                userId, resourceId, actionId,
                It.IsAny<Dictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AbacAuthResult
            {
                Decision = AuthorizationDecision.Permit
            });

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ResolvesActionIdFromHttpMethod()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(userId);

        var httpContext = CreateHttpContext("POST");
        httpContext.Request.RouteValues["resourceId"] = resourceId.ToString();

        // MapHttpMethodToActionCode("POST") => "create"
        _actionRepoMock.Setup(r => r.GetByCodeAsync("create", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActionEntity { Id = actionId, Name = "Create", Code = "create" });

        var requirement = new AbacRequirement(resourceId, null);
        var context = CreateContext(requirement, user, httpContext);

        _accessControlServiceMock.Setup(s => s.CheckAccessAsync(
                userId, resourceId, actionId,
                It.IsAny<Dictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AbacAuthResult
            {
                Decision = AuthorizationDecision.Permit
            });

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenCannotResolveResourceId_DoesNotSucceed()
    {
        var userId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(userId);
        var httpContext = CreateHttpContext("GET"); // No ids in route

        var requirement = new AbacRequirement(null, Guid.NewGuid());
        var context = CreateContext(requirement, user, httpContext);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenCannotResolveActionId_DoesNotSucceed()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(userId);

        var httpContext = CreateHttpContext("OPTIONS"); // Unknown method
        httpContext.Request.RouteValues["resourceId"] = resourceId.ToString();

        var requirement = new AbacRequirement(resourceId, null);
        var context = CreateContext(requirement, user, httpContext);

        await _sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData("GET", "read")]
    [InlineData("POST", "create")]
    [InlineData("PUT", "update")]
    [InlineData("PATCH", "update")]
    [InlineData("DELETE", "delete")]
    public async Task HandleRequirementAsync_MapsHttpMethodToActionCode(string httpMethod, string expectedCode)
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(userId);

        var httpContext = CreateHttpContext(httpMethod);
        httpContext.Request.RouteValues["resourceId"] = resourceId.ToString();

        _actionRepoMock.Setup(r => r.GetByCodeAsync(expectedCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActionEntity { Id = actionId, Name = expectedCode, Code = expectedCode });

        var requirement = new AbacRequirement(resourceId, null);
        var context = CreateContext(requirement, user, httpContext);

        _accessControlServiceMock.Setup(s => s.CheckAccessAsync(
                userId, resourceId, actionId,
                It.IsAny<Dictionary<string, object?>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AbacAuthResult
            {
                Decision = AuthorizationDecision.Permit
            });

        await _sut.HandleAsync(context);

        _actionRepoMock.Verify(r => r.GetByCodeAsync(expectedCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void AbacRequirement_DefaultConstructor_HasNullIds()
    {
        var req = new AbacRequirement();

        req.ResourceId.Should().BeNull();
        req.ActionId.Should().BeNull();
    }

    [Fact]
    public void AbacRequirement_WithIds_StoresValues()
    {
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var req = new AbacRequirement(resourceId, actionId);

        req.ResourceId.Should().Be(resourceId);
        req.ActionId.Should().Be(actionId);
    }
}
