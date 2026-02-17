using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using AutoMapper;
using Sistema.ABAC.Application.Mappings;
using ActionEntity = Sistema.ABAC.Domain.Entities.Action;

namespace Sistema.ABAC.Tests.Application.Services;

public class ActionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IActionRepository> _actionRepoMock;
    private readonly IMapper _mapper;
    private readonly ActionService _sut;

    public ActionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _actionRepoMock = new Mock<IActionRepository>();
        _unitOfWorkMock.SetupGet(u => u.Actions).Returns(_actionRepoMock.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new ActionService(
            _unitOfWorkMock.Object,
            _mapper,
            NullLogger<ActionService>.Instance);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenActionExists_ReturnsActionDto()
    {
        var actionId = Guid.NewGuid();
        var action = new ActionEntity { Id = actionId, Name = "Read", Code = "read", Description = "Read action" };

        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(action);

        var result = await _sut.GetByIdAsync(actionId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(actionId);
        result.Name.Should().Be("Read");
        result.Code.Should().Be("read");
    }

    [Fact]
    public async Task GetByIdAsync_WhenActionNotFound_ReturnsNull()
    {
        var actionId = Guid.NewGuid();
        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionEntity?)null);

        var result = await _sut.GetByIdAsync(actionId);

        result.Should().BeNull();
    }

    #endregion

    #region GetByCodeAsync

    [Fact]
    public async Task GetByCodeAsync_WhenActionExists_ReturnsActionDto()
    {
        var action = new ActionEntity { Id = Guid.NewGuid(), Name = "Write", Code = "write" };

        _actionRepoMock
            .Setup(r => r.GetByCodeAsync("write", It.IsAny<CancellationToken>()))
            .ReturnsAsync(action);

        var result = await _sut.GetByCodeAsync("write");

        result.Should().NotBeNull();
        result!.Code.Should().Be("write");
    }

    [Fact]
    public async Task GetByCodeAsync_WhenCodeIsNullOrEmpty_ReturnsNull()
    {
        var result = await _sut.GetByCodeAsync("");

        result.Should().BeNull();
        _actionRepoMock.Verify(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByCodeAsync_WhenCodeIsWhitespace_ReturnsNull()
    {
        var result = await _sut.GetByCodeAsync("   ");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_WhenActionNotFound_ReturnsNull()
    {
        _actionRepoMock
            .Setup(r => r.GetByCodeAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionEntity?)null);

        var result = await _sut.GetByCodeAsync("nonexistent");

        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        var actions = new List<ActionEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Alpha", Code = "alpha", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Beta", Code = "beta", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Gamma", Code = "gamma", CreatedAt = DateTime.UtcNow }
        };

        _actionRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        var result = await _sut.GetAllAsync(page: 1, pageSize: 10);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_FiltersResults()
    {
        var actions = new List<ActionEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Read", Code = "read", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Write", Code = "write", CreatedAt = DateTime.UtcNow }
        };

        _actionRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        var result = await _sut.GetAllAsync(searchTerm: "read");

        result.Items.Should().HaveCount(1);
        result.Items.First().Code.Should().Be("read");
    }

    [Fact]
    public async Task GetAllAsync_WithPaginationDefaults_NormalizesInvalidValues()
    {
        _actionRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActionEntity>());

        var result = await _sut.GetAllAsync(page: -1, pageSize: 0);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllAsync_WithMaxPageSize_CapsAt100()
    {
        _actionRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ActionEntity>());

        var result = await _sut.GetAllAsync(pageSize: 500);

        result.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetAllAsync_SortByCode_ReturnsSortedResults()
    {
        var actions = new List<ActionEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Zeta", Code = "zeta", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Alpha", Code = "alpha", CreatedAt = DateTime.UtcNow }
        };

        _actionRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        var result = await _sut.GetAllAsync(sortBy: "code", sortDescending: false);

        result.Items.First().Code.Should().Be("alpha");
    }

    [Fact]
    public async Task GetAllAsync_SortByCreatedAtDescending_ReturnsSortedResults()
    {
        var actions = new List<ActionEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Old", Code = "old", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), Name = "New", Code = "new", CreatedAt = DateTime.UtcNow }
        };

        _actionRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        var result = await _sut.GetAllAsync(sortBy: "createdat", sortDescending: true);

        result.Items.First().Code.Should().Be("new");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WhenCodeIsUnique_CreatesAction()
    {
        var createDto = new CreateActionDto { Name = "Delete", Code = "delete", Description = "Delete action" };

        _actionRepoMock
            .Setup(r => r.GetByCodeAsync("delete", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionEntity?)null);

        _actionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ActionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionEntity e, CancellationToken _) => e);

        var result = await _sut.CreateAsync(createDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Delete");
        result.Code.Should().Be("delete");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenCodeAlreadyExists_ThrowsValidationException()
    {
        var createDto = new CreateActionDto { Name = "Read", Code = "read" };

        _actionRepoMock
            .Setup(r => r.GetByCodeAsync("read", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ActionEntity { Id = Guid.NewGuid(), Name = "Read", Code = "read" });

        var act = async () => await _sut.CreateAsync(createDto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenActionExists_UpdatesAndReturns()
    {
        var actionId = Guid.NewGuid();
        var existingAction = new ActionEntity { Id = actionId, Name = "Read", Code = "read" };
        var updateDto = new UpdateActionDto { Name = "Read Updated", Description = "Updated description" };

        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAction);

        var result = await _sut.UpdateAsync(actionId, updateDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Read Updated");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenActionNotFound_ThrowsNotFoundException()
    {
        var actionId = Guid.NewGuid();
        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionEntity?)null);

        var act = async () => await _sut.UpdateAsync(actionId, new UpdateActionDto { Name = "Test" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyDescription_SetsDescriptionToNull()
    {
        var actionId = Guid.NewGuid();
        var existingAction = new ActionEntity { Id = actionId, Name = "Read", Code = "read", Description = "Old desc" };
        var updateDto = new UpdateActionDto { Name = "Read", Description = "   " };

        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAction);

        await _sut.UpdateAsync(actionId, updateDto);

        existingAction.Description.Should().BeNull();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenActionExists_SoftDeletes()
    {
        var actionId = Guid.NewGuid();
        var action = new ActionEntity { Id = actionId, Name = "Read", Code = "read" };

        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(action);

        _actionRepoMock
            .Setup(r => r.GetWithPoliciesAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(action);

        _actionRepoMock
            .Setup(r => r.GetAccessLogsAsync(actionId, 0, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AccessLog>());

        var result = await _sut.DeleteAsync(actionId);

        result.Should().BeTrue();
        action.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenActionNotFound_ThrowsNotFoundException()
    {
        var actionId = Guid.NewGuid();
        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionEntity?)null);

        var act = async () => await _sut.DeleteAsync(actionId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenActionHasActivePolicies_ThrowsValidationException()
    {
        var actionId = Guid.NewGuid();
        var action = new ActionEntity
        {
            Id = actionId,
            Name = "Read",
            Code = "read",
            PolicyActions = new List<PolicyAction>
            {
                new() { Policy = new Policy { IsActive = true } }
            }
        };

        _actionRepoMock
            .Setup(r => r.GetByIdAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(action);

        _actionRepoMock
            .Setup(r => r.GetWithPoliciesAsync(actionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(action);

        var act = async () => await _sut.DeleteAsync(actionId);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region ExistsByCodeAsync

    [Fact]
    public async Task ExistsByCodeAsync_WhenCodeExists_ReturnsTrue()
    {
        _actionRepoMock
            .Setup(r => r.CodeExistsAsync("read", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.ExistsByCodeAsync("read");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByCodeAsync_WhenCodeIsEmpty_ReturnsFalse()
    {
        var result = await _sut.ExistsByCodeAsync("");

        result.Should().BeFalse();
    }

    #endregion
}
