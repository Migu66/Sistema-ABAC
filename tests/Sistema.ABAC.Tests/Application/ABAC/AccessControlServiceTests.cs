using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.Services.ABAC;
using Sistema.ABAC.Domain.Interfaces;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Tests.Application.ABAC;

public class AccessControlServiceTests
{
    [Fact]
    public async Task CheckAccessAsync_WhenActionDoesNotExist_ThrowsNotFoundException()
    {
        var actionId = Guid.NewGuid();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var actionsRepositoryMock = new Mock<IActionRepository>();
        var attributeCollectorMock = new Mock<IAttributeCollectorService>();
        var policyEvaluatorMock = new Mock<IPolicyEvaluator>();

        actionsRepositoryMock
            .Setup(x => x.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionEntity?)null);

        unitOfWorkMock.SetupGet(x => x.Actions).Returns(actionsRepositoryMock.Object);

        var sut = new AccessControlService(
            attributeCollectorMock.Object,
            policyEvaluatorMock.Object,
            unitOfWorkMock.Object,
            NullLogger<AccessControlService>.Instance);

        var act = async () => await sut.CheckAccessAsync(Guid.NewGuid(), Guid.NewGuid(), actionId);

        await act.Should().ThrowAsync<NotFoundException>();

        attributeCollectorMock.Verify(
            x => x.CollectSubjectAttributesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        policyEvaluatorMock.Verify(
            x => x.EvaluateAsync(It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckAccessAsync_WhenActionIsSoftDeleted_ThrowsNotFoundException()
    {
        var actionId = Guid.NewGuid();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var actionsRepositoryMock = new Mock<IActionRepository>();
        var attributeCollectorMock = new Mock<IAttributeCollectorService>();
        var policyEvaluatorMock = new Mock<IPolicyEvaluator>();

        actionsRepositoryMock
            .Setup(x => x.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActionEntity { Id = actionId, Name = "Read", Code = "read", IsDeleted = true });

        unitOfWorkMock.SetupGet(x => x.Actions).Returns(actionsRepositoryMock.Object);

        var sut = new AccessControlService(
            attributeCollectorMock.Object,
            policyEvaluatorMock.Object,
            unitOfWorkMock.Object,
            NullLogger<AccessControlService>.Instance);

        var act = async () => await sut.CheckAccessAsync(Guid.NewGuid(), Guid.NewGuid(), actionId);

        await act.Should().ThrowAsync<NotFoundException>();

        attributeCollectorMock.Verify(
            x => x.CollectSubjectAttributesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckAccessAsync_WhenPolicyEvaluatorPermits_ReturnsPermitAndBuildsEvaluationContext()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var actionCode = "read";

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var actionsRepositoryMock = new Mock<IActionRepository>();
        var attributeCollectorMock = new Mock<IAttributeCollectorService>();
        var policyEvaluatorMock = new Mock<IPolicyEvaluator>();

        actionsRepositoryMock
            .Setup(x => x.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActionEntity { Id = actionId, Name = "Read", Code = actionCode, IsDeleted = false });

        var subject = new Dictionary<string, object?> { ["department"] = "IT" };
        var resource = new Dictionary<string, object?> { ["classification"] = "internal" };
        var environment = new Dictionary<string, object?> { ["ipAddress"] = "127.0.0.1" };

        attributeCollectorMock
            .Setup(x => x.CollectSubjectAttributesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subject);
        attributeCollectorMock
            .Setup(x => x.CollectResourceAttributesAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        attributeCollectorMock
            .Setup(x => x.CollectEnvironmentAttributesAsync(It.IsAny<IDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(environment);

        EvaluationContext? capturedContext = null;
        policyEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()))
            .Callback<EvaluationContext, CancellationToken>((ctx, _) => capturedContext = ctx)
            .ReturnsAsync(true);

        unitOfWorkMock.SetupGet(x => x.Actions).Returns(actionsRepositoryMock.Object);

        var sut = new AccessControlService(
            attributeCollectorMock.Object,
            policyEvaluatorMock.Object,
            unitOfWorkMock.Object,
            NullLogger<AccessControlService>.Instance);

        var inputContext = new Dictionary<string, object?> { ["requestId"] = "req-1" };

        var result = await sut.CheckAccessAsync(userId, resourceId, actionId, inputContext);

        result.Decision.Should().Be(AuthorizationDecision.Permit);
        result.Reason.Should().Contain("Acceso permitido");

        capturedContext.Should().NotBeNull();
        capturedContext!.Subject["userId"].Should().Be(userId);
        capturedContext.Resource["resourceId"].Should().Be(resourceId);
        capturedContext.Action["actionId"].Should().Be(actionId);
        capturedContext.Action["code"].Should().Be(actionCode);
        capturedContext.Environment["ipAddress"].Should().Be("127.0.0.1");
    }

    [Fact]
    public async Task CheckAccessAsync_WhenPolicyEvaluatorDenies_ReturnsDeny()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var actionsRepositoryMock = new Mock<IActionRepository>();
        var attributeCollectorMock = new Mock<IAttributeCollectorService>();
        var policyEvaluatorMock = new Mock<IPolicyEvaluator>();

        actionsRepositoryMock
            .Setup(x => x.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActionEntity { Id = actionId, Name = "Delete", Code = "delete", IsDeleted = false });

        attributeCollectorMock
            .Setup(x => x.CollectSubjectAttributesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, object?>());
        attributeCollectorMock
            .Setup(x => x.CollectResourceAttributesAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, object?>());
        attributeCollectorMock
            .Setup(x => x.CollectEnvironmentAttributesAsync(It.IsAny<IDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, object?>());

        policyEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<EvaluationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        unitOfWorkMock.SetupGet(x => x.Actions).Returns(actionsRepositoryMock.Object);

        var sut = new AccessControlService(
            attributeCollectorMock.Object,
            policyEvaluatorMock.Object,
            unitOfWorkMock.Object,
            NullLogger<AccessControlService>.Instance);

        var result = await sut.CheckAccessAsync(userId, resourceId, actionId);

        result.Decision.Should().Be(AuthorizationDecision.Deny);
        result.Reason.Should().Contain("Acceso denegado");
    }

    [Fact]
    public async Task CheckAccessAsync_PassesCancellationTokenToDependencies()
    {
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var actionId = Guid.NewGuid();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var actionsRepositoryMock = new Mock<IActionRepository>();
        var attributeCollectorMock = new Mock<IAttributeCollectorService>();
        var policyEvaluatorMock = new Mock<IPolicyEvaluator>();

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        actionsRepositoryMock
            .Setup(x => x.GetByIdAsync(actionId, cancellationToken))
            .ReturnsAsync(new ActionEntity { Id = actionId, Name = "Read", Code = "read", IsDeleted = false });

        attributeCollectorMock
            .Setup(x => x.CollectSubjectAttributesAsync(userId, cancellationToken))
            .ReturnsAsync(new Dictionary<string, object?>());
        attributeCollectorMock
            .Setup(x => x.CollectResourceAttributesAsync(resourceId, cancellationToken))
            .ReturnsAsync(new Dictionary<string, object?>());
        attributeCollectorMock
            .Setup(x => x.CollectEnvironmentAttributesAsync(It.IsAny<IDictionary<string, object?>>(), cancellationToken))
            .ReturnsAsync(new Dictionary<string, object?>());

        policyEvaluatorMock
            .Setup(x => x.EvaluateAsync(It.IsAny<EvaluationContext>(), cancellationToken))
            .ReturnsAsync(true);

        unitOfWorkMock.SetupGet(x => x.Actions).Returns(actionsRepositoryMock.Object);

        var sut = new AccessControlService(
            attributeCollectorMock.Object,
            policyEvaluatorMock.Object,
            unitOfWorkMock.Object,
            NullLogger<AccessControlService>.Instance);

        var result = await sut.CheckAccessAsync(userId, resourceId, actionId, cancellationToken: cancellationToken);

        result.Decision.Should().Be(AuthorizationDecision.Permit);
        actionsRepositoryMock.Verify(x => x.GetByIdAsync(actionId, cancellationToken), Times.Once);
        attributeCollectorMock.Verify(x => x.CollectSubjectAttributesAsync(userId, cancellationToken), Times.Once);
        attributeCollectorMock.Verify(x => x.CollectResourceAttributesAsync(resourceId, cancellationToken), Times.Once);
        policyEvaluatorMock.Verify(x => x.EvaluateAsync(It.IsAny<EvaluationContext>(), cancellationToken), Times.Once);
    }
}
