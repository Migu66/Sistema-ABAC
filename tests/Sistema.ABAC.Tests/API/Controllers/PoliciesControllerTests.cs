using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.API.Controllers;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Tests.API.Controllers;

public class PoliciesControllerTests
{
    private readonly Mock<IPolicyService> _serviceMock;
    private readonly PoliciesController _sut;

    public PoliciesControllerTests()
    {
        _serviceMock = new Mock<IPolicyService>();
        _sut = new PoliciesController(_serviceMock.Object, NullLogger<PoliciesController>.Instance);
    }

    #region CRUD

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var paged = new PagedResultDto<PolicyDto> { Items = new List<PolicyDto>(), TotalCount = 0, Page = 1, PageSize = 10 };
        _serviceMock.Setup(s => s.GetAllAsync(1, 10, null, null, null, "Priority", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicyDto { Id = id, Name = "Policy1" });

        var result = await _sut.GetById(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PolicyDto?)null);

        var result = await _sut.GetById(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetActive_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetActivePoliciesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PolicyDto>());

        var result = await _sut.GetActive();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByAction_ReturnsOk()
    {
        var actionId = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetPoliciesForActionAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PolicyDto>());

        var result = await _sut.GetByAction(actionId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var dto = new CreatePolicyDto { Name = "New", Effect = PolicyEffect.Permit, Priority = 1 };
        var created = new PolicyDto { Id = Guid.NewGuid(), Name = "New" };

        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _sut.Create(dto);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var dto = new UpdatePolicyDto { Name = "Updated" };
        _serviceMock.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicyDto { Id = id, Name = "Updated" });

        var result = await _sut.Update(id, dto);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Activation

    [Fact]
    public async Task Activate_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ActivateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicyDto { Id = id, IsActive = true });

        var result = await _sut.Activate(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Deactivate_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeactivateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicyDto { Id = id, IsActive = false });

        var result = await _sut.Deactivate(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Conditions

    [Fact]
    public async Task GetConditions_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetConditionsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PolicyConditionDto>());

        var result = await _sut.GetConditions(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AddCondition_ReturnsCreatedAtAction()
    {
        var id = Guid.NewGuid();
        var dto = new CreatePolicyConditionDto { AttributeType = "Subject", AttributeKey = "level", Operator = Domain.Enums.OperatorType.Equals, ExpectedValue = "5" };
        var created = new PolicyConditionDto { Id = Guid.NewGuid() };

        _serviceMock.Setup(s => s.AddConditionAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _sut.AddCondition(id, dto);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task UpdateCondition_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var condId = Guid.NewGuid();
        var dto = new UpdatePolicyConditionDto { AttributeType = "Subject", AttributeKey = "level", Operator = Domain.Enums.OperatorType.Equals, ExpectedValue = "10" };
        _serviceMock.Setup(s => s.UpdateConditionAsync(id, condId, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicyConditionDto { Id = condId });

        var result = await _sut.UpdateCondition(id, condId, dto);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RemoveCondition_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var condId = Guid.NewGuid();
        _serviceMock.Setup(s => s.RemoveConditionAsync(id, condId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.RemoveCondition(id, condId);

        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Actions

    [Fact]
    public async Task GetActions_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetActionsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActionDto>());

        var result = await _sut.GetActions(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AssociateActions_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var actionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _serviceMock.Setup(s => s.AssociateActionsAsync(id, actionIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.AssociateActions(id, actionIds);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DisassociateAction_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var actionId = Guid.NewGuid();

        _serviceMock.Setup(s => s.DisassociateActionAsync(id, actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.DisassociateAction(id, actionId);

        result.Should().BeOfType<NoContentResult>();
    }

    #endregion

    #region Validation & Statistics

    [Fact]
    public async Task Validate_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.ValidatePolicyAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.Validate(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStatistics_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetStatisticsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PolicyStatisticsDto());

        var result = await _sut.GetStatistics(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}
