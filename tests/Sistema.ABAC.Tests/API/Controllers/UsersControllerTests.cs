using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.API.Controllers;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.DTOs.Common;
using Sistema.ABAC.Application.Services;

namespace Sistema.ABAC.Tests.API.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _serviceMock;
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _serviceMock = new Mock<IUserService>();
        _sut = new UsersController(_serviceMock.Object, NullLogger<UsersController>.Instance);
    }

    #region GetAll

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResult()
    {
        var paged = new PagedResultDto<UserDto>
        {
            Items = new List<UserDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _serviceMock.Setup(s => s.GetAllAsync(1, 10, null, null, null, "UserName", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDto { Id = id, UserName = "testuser" });

        var result = await _sut.GetById(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        var result = await _sut.GetById(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WithIncludeAttributes_PassesParameter()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDto { Id = id, UserName = "testuser" });

        var result = await _sut.GetById(id, includeAttributes: true);

        result.Result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.GetByIdAsync(id, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_WhenValid_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateUserDto { FullName = "Updated Name" };
        _serviceMock.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDto { Id = id, UserName = "user", FullName = "Updated Name" });

        var result = await _sut.Update(id, dto);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateUserDto { FullName = "Test" };
        _serviceMock.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Usuario", id));

        var result = await _sut.Update(id, dto);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenValidationError_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateUserDto();
        var errors = new Dictionary<string, string[]>
        {
            { "FullName", new[] { "Campo requerido" } }
        };
        _serviceMock.Setup(s => s.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(errors));

        var result = await _sut.Update(id, dto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Usuario", id));

        var result = await _sut.Delete(id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetUserAttributes

    [Fact]
    public async Task GetUserAttributes_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var attrs = new List<UserAttributeDto> { new() { AttributeId = Guid.NewGuid(), Value = "5" } };

        _serviceMock.Setup(s => s.GetUserAttributesAsync(id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attrs);

        var result = await _sut.GetUserAttributes(id);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(attrs);
    }

    [Fact]
    public async Task GetUserAttributes_WhenNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetUserAttributesAsync(id, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Usuario", id));

        var result = await _sut.GetUserAttributes(id);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region AssignAttribute

    [Fact]
    public async Task AssignAttribute_WhenValid_ReturnsCreatedAtAction()
    {
        var id = Guid.NewGuid();
        var assignDto = new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = "10" };
        var result_attr = new UserAttributeDto { AttributeId = assignDto.AttributeId, Value = "10" };

        _serviceMock.Setup(s => s.AssignAttributeAsync(id, assignDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result_attr);

        var result = await _sut.AssignAttribute(id, assignDto);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task AssignAttribute_WhenNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var assignDto = new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = "10" };

        _serviceMock.Setup(s => s.AssignAttributeAsync(id, assignDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Usuario", id));

        var result = await _sut.AssignAttribute(id, assignDto);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AssignAttribute_WhenValidationError_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var assignDto = new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = "bad" };
        var errors = new Dictionary<string, string[]>
        {
            { "Value", new[] { "Valor inválido" } }
        };

        _serviceMock.Setup(s => s.AssignAttributeAsync(id, assignDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(errors));

        var result = await _sut.AssignAttribute(id, assignDto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region RemoveAttribute

    [Fact]
    public async Task RemoveAttribute_WhenValid_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        var attrId = Guid.NewGuid();
        _serviceMock.Setup(s => s.RemoveAttributeAsync(id, attrId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.RemoveAttribute(id, attrId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemoveAttribute_WhenNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var attrId = Guid.NewGuid();
        _serviceMock.Setup(s => s.RemoveAttributeAsync(id, attrId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Atributo", attrId));

        var result = await _sut.RemoveAttribute(id, attrId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
