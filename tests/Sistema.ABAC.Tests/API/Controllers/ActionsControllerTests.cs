using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.API.Controllers;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.Tests.API.Controllers;

public class ActionsControllerTests
{
    private readonly Mock<IActionService> _serviceMock;
    private readonly ActionsController _sut;

    public ActionsControllerTests()
    {
        _serviceMock = new Mock<IActionService>();
        _sut = new ActionsController(_serviceMock.Object, NullLogger<ActionsController>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var paged = new PagedResultDto<ActionDto> { Items = new List<ActionDto>(), TotalCount = 0, Page = 1, PageSize = 10 };
        _serviceMock.Setup(s => s.GetAllAsync(1, 10, null, "Name", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(paged);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var dto = new ActionDto { Id = id, Name = "Read", Code = "read" };
        _serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.GetById(id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionDto?)null);

        var result = await _sut.GetById(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByCode_WhenFound_ReturnsOk()
    {
        var dto = new ActionDto { Id = Guid.NewGuid(), Name = "Write", Code = "write" };
        _serviceMock.Setup(s => s.GetByCodeAsync("write", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.GetByCode("write");

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetByCode_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionDto?)null);

        var result = await _sut.GetByCode("missing");

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var createDto = new CreateActionDto { Name = "Delete", Code = "delete" };
        var created = new ActionDto { Id = Guid.NewGuid(), Name = "Delete", Code = "delete" };

        _serviceMock.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _sut.Create(createDto);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(created);
        createdResult.ActionName.Should().Be(nameof(ActionsController.GetById));
    }

    [Fact]
    public async Task Update_ReturnsOkWithUpdated()
    {
        var id = Guid.NewGuid();
        var updateDto = new UpdateActionDto { Name = "Updated" };
        var updated = new ActionDto { Id = id, Name = "Updated", Code = "read" };

        _serviceMock.Setup(s => s.UpdateAsync(id, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        var result = await _sut.Update(id, updateDto);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
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

    [Fact]
    public async Task ExistsByCode_ReturnsOk()
    {
        _serviceMock.Setup(s => s.ExistsByCodeAsync("read", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.ExistsByCode("read");

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}
