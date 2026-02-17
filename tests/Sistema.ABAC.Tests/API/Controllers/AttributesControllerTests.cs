using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.API.Controllers;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.Tests.API.Controllers;

public class AttributesControllerTests
{
    private readonly Mock<IAttributeService> _serviceMock;
    private readonly AttributesController _sut;

    public AttributesControllerTests()
    {
        _serviceMock = new Mock<IAttributeService>();
        _sut = new AttributesController(_serviceMock.Object, NullLogger<AttributesController>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var paged = new PagedResultDto<AttributeDto> { Items = new List<AttributeDto>(), TotalCount = 0, Page = 1, PageSize = 10 };
        _serviceMock.Setup(s => s.GetAllAsync(1, 10, null, null, "Name", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AttributeDto { Id = id, Name = "Level" });

        var result = await _sut.GetById(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttributeDto?)null);

        var result = await _sut.GetById(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var dto = new CreateAttributeDto { Name = "Level", Key = "level", Type = Domain.Enums.AttributeType.Number };
        var created = new AttributeDto { Id = Guid.NewGuid(), Name = "Level" };

        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _sut.Create(dto);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateAttributeDto { Name = "Updated" };
        _serviceMock.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AttributeDto { Id = id, Name = "Updated" });

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
}
