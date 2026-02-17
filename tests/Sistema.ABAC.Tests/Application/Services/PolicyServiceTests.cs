using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Enums;
using Sistema.ABAC.Domain.Interfaces;
using AutoMapper;
using Sistema.ABAC.Application.Mappings;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Tests.Application.Services;

public class PolicyServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPolicyRepository> _policyRepoMock;
    private readonly Mock<IActionRepository> _actionRepoMock;
    private readonly Mock<IRepository<PolicyCondition>> _conditionRepoMock;
    private readonly Mock<IRepository<PolicyAction>> _policyActionRepoMock;
    private readonly IMapper _mapper;
    private readonly PolicyService _sut;

    public PolicyServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _policyRepoMock = new Mock<IPolicyRepository>();
        _actionRepoMock = new Mock<IActionRepository>();
        _conditionRepoMock = new Mock<IRepository<PolicyCondition>>();
        _policyActionRepoMock = new Mock<IRepository<PolicyAction>>();

        _unitOfWorkMock.SetupGet(u => u.Policies).Returns(_policyRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Actions).Returns(_actionRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.PolicyConditions).Returns(_conditionRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.PolicyActions).Returns(_policyActionRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new PolicyService(
            _unitOfWorkMock.Object,
            _mapper,
            NullLogger<PolicyService>.Instance);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsPolicyDto()
    {
        var id = Guid.NewGuid();
        var policy = new Policy { Id = id, Name = "Test Policy", Effect = PolicyEffect.Permit, Priority = 100 };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Policy");
    }

    [Fact]
    public async Task GetByIdAsync_WithDetails_UsesGetWithDetailsAsync()
    {
        var id = Guid.NewGuid();
        var policy = new Policy
        {
            Id = id,
            Name = "Policy",
            Effect = PolicyEffect.Permit,
            Conditions = new List<PolicyCondition>(),
            PolicyActions = new List<PolicyAction>()
        };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.GetByIdAsync(id, includeDetails: true);

        result.Should().NotBeNull();
        _policyRepoMock.Verify(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _policyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var policies = new List<Policy>
        {
            new() { Id = Guid.NewGuid(), Name = "Policy A", Effect = PolicyEffect.Permit, Priority = 100, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Policy B", Effect = PolicyEffect.Deny, Priority = 200, CreatedAt = DateTime.UtcNow }
        };

        _policyRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var result = await _sut.GetAllAsync();

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_Filters()
    {
        var policies = new List<Policy>
        {
            new() { Id = Guid.NewGuid(), Name = "Admin Policy", Effect = PolicyEffect.Permit, Priority = 100, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "User Policy", Effect = PolicyEffect.Deny, Priority = 50, CreatedAt = DateTime.UtcNow }
        };

        _policyRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var result = await _sut.GetAllAsync(searchTerm: "Admin");

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Admin Policy");
    }

    [Fact]
    public async Task GetAllAsync_WithEffectFilter_FiltersCorrectly()
    {
        var policies = new List<Policy>
        {
            new() { Id = Guid.NewGuid(), Name = "P1", Effect = PolicyEffect.Permit, Priority = 100, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "P2", Effect = PolicyEffect.Deny, Priority = 50, CreatedAt = DateTime.UtcNow }
        };

        _policyRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var result = await _sut.GetAllAsync(effect: PolicyEffect.Deny);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("P2");
    }

    [Fact]
    public async Task GetAllAsync_WithIsActiveFilter_FiltersCorrectly()
    {
        var policies = new List<Policy>
        {
            new() { Id = Guid.NewGuid(), Name = "Active", IsActive = true, Priority = 100, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Inactive", IsActive = false, Priority = 50, CreatedAt = DateTime.UtcNow }
        };

        _policyRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var result = await _sut.GetAllAsync(isActive: true);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetAllAsync_SortByName_ReturnsSorted()
    {
        var policies = new List<Policy>
        {
            new() { Id = Guid.NewGuid(), Name = "Zeta", Priority = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Alpha", Priority = 2, CreatedAt = DateTime.UtcNow }
        };

        _policyRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var result = await _sut.GetAllAsync(sortBy: "name", sortDescending: false);

        result.Items.First().Name.Should().Be("Alpha");
    }

    #endregion

    #region GetActivePoliciesAsync

    [Fact]
    public async Task GetActivePoliciesAsync_ReturnsMappedList()
    {
        var policies = new List<Policy>
        {
            new() { Id = Guid.NewGuid(), Name = "Active 1", IsActive = true }
        };

        _policyRepoMock
            .Setup(r => r.GetActivePoliciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var result = await _sut.GetActivePoliciesAsync();

        result.Should().HaveCount(1);
    }

    #endregion

    #region GetPoliciesForActionAsync

    [Fact]
    public async Task GetPoliciesForActionAsync_ReturnsMappedList()
    {
        var actionId = Guid.NewGuid();
        var policies = new List<Policy>
        {
            new() { Id = Guid.NewGuid(), Name = "Policy X" }
        };

        _policyRepoMock
            .Setup(r => r.GetActivePoliciesForActionAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);

        var result = await _sut.GetPoliciesForActionAsync(actionId);

        result.Should().HaveCount(1);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithoutConditionsOrActions_CreatesPolicy()
    {
        var dto = new CreatePolicyDto { Name = "New Policy", Effect = PolicyEffect.Permit, Priority = 50 };

        _policyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy p, CancellationToken _) => p);

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Policy
            {
                Id = id,
                Name = "New Policy",
                Effect = PolicyEffect.Permit,
                Priority = 50,
                Conditions = new List<PolicyCondition>(),
                PolicyActions = new List<PolicyAction>()
            });

        var result = await _sut.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Policy");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithActions_ValidatesActionsExist()
    {
        var actionId = Guid.NewGuid();
        var dto = new CreatePolicyDto
        {
            Name = "Policy",
            Effect = PolicyEffect.Permit,
            ActionIds = new List<Guid> { actionId }
        };

        _policyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Policy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy p, CancellationToken _) => p);

        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionEntity?)null);

        var act = async () => await _sut.CreateAsync(dto);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesPolicy()
    {
        var id = Guid.NewGuid();
        var existing = new Policy { Id = id, Name = "Old", Effect = PolicyEffect.Permit, Priority = 100 };
        var dto = new UpdatePolicyDto { Name = "Updated", Effect = PolicyEffect.Deny, Priority = 200, IsActive = true };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(id, dto);

        result.Name.Should().Be("Updated");
        result.Effect.Should().Be(PolicyEffect.Deny);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _policyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), new UpdatePolicyDto { Name = "Test" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenExists_SoftDeletes()
    {
        var id = Guid.NewGuid();
        var policy = new Policy { Id = id, Name = "Test" };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.DeleteAsync(id);

        result.Should().BeTrue();
        policy.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var act = async () => await _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ActivateAsync / DeactivateAsync

    [Fact]
    public async Task ActivateAsync_WhenPolicyIsValid_ActivatesPolicy()
    {
        var id = Guid.NewGuid();
        var policy = new Policy { Id = id, Name = "Test", IsActive = false };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Policy
            {
                Id = id,
                Name = "Test",
                Conditions = new List<PolicyCondition> { new() },
                PolicyActions = new List<PolicyAction> { new() }
            });

        var result = await _sut.ActivateAsync(id);

        policy.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateAsync_WhenAlreadyActive_ReturnsWithoutChanges()
    {
        var id = Guid.NewGuid();
        var policy = new Policy { Id = id, Name = "Test", IsActive = true };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.ActivateAsync(id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivateAsync_WhenPolicyIsInvalid_ThrowsValidationException()
    {
        var id = Guid.NewGuid();
        var policy = new Policy { Id = id, Name = "Test", IsActive = false };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Policy { Id = id, Name = "Test", Conditions = new List<PolicyCondition>() });

        var act = async () => await _sut.ActivateAsync(id);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ActivateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _policyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var act = async () => await _sut.ActivateAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeactivateAsync_WhenActive_Deactivates()
    {
        var id = Guid.NewGuid();
        var policy = new Policy { Id = id, Name = "Test", IsActive = true };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        await _sut.DeactivateAsync(id);

        policy.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_WhenAlreadyInactive_ReturnsWithoutChanges()
    {
        var id = Guid.NewGuid();
        var policy = new Policy { Id = id, Name = "Test", IsActive = false };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        await _sut.DeactivateAsync(id);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Condition Management

    [Fact]
    public async Task GetConditionsAsync_ReturnsPolicyConditions()
    {
        var id = Guid.NewGuid();
        var policy = new Policy
        {
            Id = id,
            Name = "Test",
            Conditions = new List<PolicyCondition>
            {
                new() { Id = Guid.NewGuid(), AttributeKey = "dept", AttributeType = "Subject", ExpectedValue = "IT", Operator = OperatorType.Equals }
            }
        };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.GetConditionsAsync(id);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetConditionsAsync_WhenPolicyNotFound_ThrowsNotFoundException()
    {
        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var act = async () => await _sut.GetConditionsAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddConditionAsync_WhenPolicyExists_AddsCondition()
    {
        var policyId = Guid.NewGuid();
        var policy = new Policy { Id = policyId, Name = "Test" };
        var dto = new CreatePolicyConditionDto { AttributeType = "Subject", AttributeKey = "dept", Operator = OperatorType.Equals, ExpectedValue = "IT" };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        _conditionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<PolicyCondition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PolicyCondition c, CancellationToken _) => c);

        var result = await _sut.AddConditionAsync(policyId, dto);

        result.Should().NotBeNull();
        result.AttributeKey.Should().Be("dept");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateConditionAsync_WhenConditionBelongsToPolicy_Updates()
    {
        var policyId = Guid.NewGuid();
        var conditionId = Guid.NewGuid();
        var policy = new Policy
        {
            Id = policyId,
            Name = "Test",
            Conditions = new List<PolicyCondition>
            {
                new() { Id = conditionId, AttributeKey = "old_key", Operator = OperatorType.Equals }
            }
        };
        var dto = new UpdatePolicyConditionDto { AttributeType = "Subject", AttributeKey = "new_key", Operator = OperatorType.Contains, ExpectedValue = "val" };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.UpdateConditionAsync(policyId, conditionId, dto);

        result.AttributeKey.Should().Be("new_key");
    }

    [Fact]
    public async Task UpdateConditionAsync_WhenConditionNotInPolicy_ThrowsNotFoundException()
    {
        var policyId = Guid.NewGuid();
        var policy = new Policy { Id = policyId, Name = "Test", Conditions = new List<PolicyCondition>() };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var act = async () => await _sut.UpdateConditionAsync(policyId, Guid.NewGuid(), new UpdatePolicyConditionDto());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RemoveConditionAsync_WhenValid_RemovesCondition()
    {
        var policyId = Guid.NewGuid();
        var conditionId = Guid.NewGuid();
        var policy = new Policy
        {
            Id = policyId,
            Name = "Test",
            IsActive = false,
            Conditions = new List<PolicyCondition>
            {
                new() { Id = conditionId }
            }
        };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.RemoveConditionAsync(policyId, conditionId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveConditionAsync_WhenLastConditionOnActivePolicy_ThrowsValidationException()
    {
        var policyId = Guid.NewGuid();
        var conditionId = Guid.NewGuid();
        var policy = new Policy
        {
            Id = policyId,
            Name = "Test",
            IsActive = true,
            Conditions = new List<PolicyCondition>
            {
                new() { Id = conditionId }
            }
        };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var act = async () => await _sut.RemoveConditionAsync(policyId, conditionId);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Action Association Management

    [Fact]
    public async Task AssociateActionsAsync_WhenActionsExist_Associates()
    {
        var policyId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var policy = new Policy { Id = policyId, Name = "Test" };

        _policyRepoMock
            .Setup(r => r.GetByIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActionEntity { Id = actionId, Name = "Read", Code = "read" });

        _policyActionRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PolicyAction>());

        _policyActionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<PolicyAction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PolicyAction pa, CancellationToken _) => pa);

        var result = await _sut.AssociateActionsAsync(policyId, new List<Guid> { actionId });

        result.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisassociateActionAsync_WhenLastActionOnActivePolicy_ThrowsValidationException()
    {
        var policyId = Guid.NewGuid();
        var actionId = Guid.NewGuid();
        var policy = new Policy
        {
            Id = policyId,
            Name = "Test",
            IsActive = true,
            PolicyActions = new List<PolicyAction>
            {
                new() { ActionId = actionId }
            }
        };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var act = async () => await _sut.DisassociateActionAsync(policyId, actionId);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region ValidatePolicyAsync

    [Fact]
    public async Task ValidatePolicyAsync_WhenHasConditionsAndActions_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var policy = new Policy
        {
            Id = id,
            Conditions = new List<PolicyCondition> { new() },
            PolicyActions = new List<PolicyAction> { new() }
        };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.ValidatePolicyAsync(id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePolicyAsync_WhenMissingConditions_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var policy = new Policy
        {
            Id = id,
            Conditions = new List<PolicyCondition>(),
            PolicyActions = new List<PolicyAction> { new() }
        };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        var result = await _sut.ValidatePolicyAsync(id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePolicyAsync_WhenPolicyNotFound_ReturnsFalse()
    {
        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var result = await _sut.ValidatePolicyAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    #endregion

    #region GetStatisticsAsync

    [Fact]
    public async Task GetStatisticsAsync_WhenPolicyExists_ReturnsStatistics()
    {
        var id = Guid.NewGuid();
        var policy = new Policy
        {
            Id = id,
            Name = "Test",
            Effect = PolicyEffect.Permit,
            Priority = 100,
            IsActive = true,
            Conditions = new List<PolicyCondition> { new(), new() },
            PolicyActions = new List<PolicyAction> { new() }
        };

        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        _policyRepoMock
            .Setup(r => r.GetPolicyApplicationCountAsync(id, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var result = await _sut.GetStatisticsAsync(id);

        result.PolicyId.Should().Be(id);
        result.ConditionsCount.Should().Be(2);
        result.ActionsCount.Should().Be(1);
        result.AccessLogsCount.Should().Be(42);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatisticsAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _policyRepoMock
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policy?)null);

        var act = async () => await _sut.GetStatisticsAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
