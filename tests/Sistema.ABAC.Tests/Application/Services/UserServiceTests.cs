using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using Sistema.ABAC.Domain.Interfaces;
using AutoMapper;
using Sistema.ABAC.Application.Mappings;
using UserEntity = Sistema.ABAC.Domain.Entities.User;
using UserAttributeEntity = Sistema.ABAC.Domain.Entities.UserAttribute;
using AttributeEntity = Sistema.ABAC.Domain.Entities.Attribute;

namespace Sistema.ABAC.Tests.Application.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IAttributeRepository> _attrRepoMock;
    private readonly Mock<IRepository<UserAttributeEntity>> _userAttrRepoMock;
    private readonly Mock<UserManager<UserEntity>> _userManagerMock;
    private readonly IMapper _mapper;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _attrRepoMock = new Mock<IAttributeRepository>();
        _userAttrRepoMock = new Mock<IRepository<UserAttributeEntity>>();

        _unitOfWorkMock.SetupGet(u => u.Users).Returns(_userRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Attributes).Returns(_attrRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.UserAttributes).Returns(_userAttrRepoMock.Object);

        var store = new Mock<IUserStore<UserEntity>>();
        _userManagerMock = new Mock<UserManager<UserEntity>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new UserService(
            _unitOfWorkMock.Object,
            _userManagerMock.Object,
            _mapper,
            NullLogger<UserService>.Instance);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsUserDto()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@test.com" };

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        var result = await _sut.GetByIdAsync(userId);

        result.Should().NotBeNull();
        result!.UserName.Should().Be("testuser");
        result.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task GetByIdAsync_WithAttributes_UsesGetWithAttributesAsync()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            UserAttributes = new List<UserAttributeEntity>()
        };

        _userRepoMock
            .Setup(r => r.GetWithAttributesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var result = await _sut.GetByIdAsync(userId, includeAttributes: true);

        result.Should().NotBeNull();
        _userRepoMock.Verify(r => r.GetWithAttributesAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _userRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesUser()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "testuser", FirstName = "Old", LastName = "Name", Email = "old@test.com" };
        var dto = new UpdateUserDto { FullName = "New Name", Email = "new@test.com" };

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.FindByEmailAsync("new@test.com"))
            .ReturnsAsync((UserEntity?)null);

        _userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var result = await _sut.UpdateAsync(userId, dto);

        result.Should().NotBeNull();
        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _userManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((UserEntity?)null);

        var act = async () => await _sut.UpdateAsync(Guid.NewGuid(), new UpdateUserDto());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenDeleted_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, IsDeleted = true };

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var act = async () => await _sut.UpdateAsync(userId, new UpdateUserDto());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailAlreadyTaken_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "old@test.com" };
        var otherUser = new UserEntity { Id = otherUserId, Email = "taken@test.com" };

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.FindByEmailAsync("taken@test.com"))
            .ReturnsAsync(otherUser);

        var act = async () => await _sut.UpdateAsync(userId, new UpdateUserDto { Email = "taken@test.com" });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenUserManagerFails_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@test.com" };

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        var act = async () => await _sut.UpdateAsync(userId, new UpdateUserDto { PhoneNumber = "123" });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_WithIsActive_SetsIsDeleted()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User", Email = "test@test.com" };
        var dto = new UpdateUserDto { IsActive = false };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        await _sut.UpdateAsync(userId, dto);

        user.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenExists_SoftDeletes()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        await _sut.DeleteAsync(userId);

        user.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _userManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((UserEntity?)null);

        var act = async () => await _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenUserManagerFails_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserName = "testuser", FirstName = "Test", LastName = "User" };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Fail" }));

        var act = async () => await _sut.DeleteAsync(userId);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region User Attributes

    [Fact]
    public async Task GetUserAttributesAsync_WhenUserExists_ReturnsAttributes()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId };
        var attrs = new List<UserAttributeEntity>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Value = "IT" }
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetActiveAttributesAsync(userId, null, It.IsAny<CancellationToken>())).ReturnsAsync(attrs);

        var result = await _sut.GetUserAttributesAsync(userId);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserAttributesAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        _userRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var act = async () => await _sut.GetUserAttributesAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetUserAttributesAsync_WithIncludeExpired_ReturnsAllAttributes()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            UserAttributes = new List<UserAttributeEntity>
            {
                new() { Id = Guid.NewGuid(), UserId = userId, Value = "val1" },
                new() { Id = Guid.NewGuid(), UserId = userId, Value = "val2" }
            }
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetWithAttributesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.GetUserAttributesAsync(userId, includeExpired: true);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task AssignAttributeAsync_WhenValid_AssignsAttribute()
    {
        var userId = Guid.NewGuid();
        var attrId = Guid.NewGuid();
        var user = new UserEntity { Id = userId };
        var attribute = new AttributeEntity { Id = attrId, Name = "Department", Key = "dept" };
        var userWithAttrs = new UserEntity { Id = userId, UserAttributes = new List<UserAttributeEntity>() };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _attrRepoMock.Setup(r => r.GetByIdAsync(attrId, It.IsAny<CancellationToken>())).ReturnsAsync(attribute);
        _userRepoMock.Setup(r => r.GetWithAttributesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userWithAttrs);

        _userAttrRepoMock
            .Setup(r => r.AddAsync(It.IsAny<UserAttributeEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAttributeEntity ua, CancellationToken _) => ua);

        _userAttrRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAttributeEntity { Id = Guid.NewGuid(), UserId = userId, AttributeId = attrId, Value = "IT" });

        var dto = new AssignUserAttributeDto { AttributeId = attrId, Value = "IT" };
        var result = await _sut.AssignAttributeAsync(userId, dto);

        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignAttributeAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        _userRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var act = async () => await _sut.AssignAttributeAsync(Guid.NewGuid(), new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = "v" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignAttributeAsync_WhenAttributeNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserEntity { Id = userId });
        _attrRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AttributeEntity?)null);

        var act = async () => await _sut.AssignAttributeAsync(userId, new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = "v" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignAttributeAsync_WhenAlreadyAssigned_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var attrId = Guid.NewGuid();
        var user = new UserEntity { Id = userId };
        var attribute = new AttributeEntity { Id = attrId, Name = "Dept", Key = "dept" };
        var userWithAttrs = new UserEntity
        {
            Id = userId,
            UserAttributes = new List<UserAttributeEntity>
            {
                new() { AttributeId = attrId, IsDeleted = false }
            }
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _attrRepoMock.Setup(r => r.GetByIdAsync(attrId, It.IsAny<CancellationToken>())).ReturnsAsync(attribute);
        _userRepoMock.Setup(r => r.GetWithAttributesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userWithAttrs);

        var act = async () => await _sut.AssignAttributeAsync(userId, new AssignUserAttributeDto { AttributeId = attrId, Value = "v" });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAttributeAsync_WhenValid_Updates()
    {
        var userId = Guid.NewGuid();
        var attrId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            UserAttributes = new List<UserAttributeEntity>
            {
                new() { Id = Guid.NewGuid(), AttributeId = attrId, Value = "old", IsDeleted = false }
            }
        };

        _userRepoMock.Setup(r => r.GetWithAttributesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var dto = new UpdateUserAttributeDto { Value = "new" };
        var result = await _sut.UpdateAttributeAsync(userId, attrId, dto);

        result.Value.Should().Be("new");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAttributeAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        _userRepoMock
            .Setup(r => r.GetWithAttributesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var act = async () => await _sut.UpdateAttributeAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateUserAttributeDto { Value = "v" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RemoveAttributeAsync_WhenValid_Removes()
    {
        var userId = Guid.NewGuid();
        var attrId = Guid.NewGuid();
        var ua = new UserAttributeEntity { Id = Guid.NewGuid(), AttributeId = attrId, IsDeleted = false };
        var user = new UserEntity { Id = userId, UserAttributes = new List<UserAttributeEntity> { ua } };

        _userRepoMock.Setup(r => r.GetWithAttributesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _sut.RemoveAttributeAsync(userId, attrId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAttributeAsync_WhenAttributeNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var user = new UserEntity { Id = userId, UserAttributes = new List<UserAttributeEntity>() };

        _userRepoMock.Setup(r => r.GetWithAttributesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var act = async () => await _sut.RemoveAttributeAsync(userId, Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetUsersByAttributeAsync

    [Fact]
    public async Task GetUsersByAttributeAsync_ReturnsMappedUsers()
    {
        var users = new List<UserEntity>
        {
            new() { Id = Guid.NewGuid(), UserName = "user1", FirstName = "U1", LastName = "L1", Email = "u1@test.com" }
        };

        _userRepoMock.Setup(r => r.GetByAttributeAsync("dept", "IT", It.IsAny<CancellationToken>())).ReturnsAsync(users);
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<UserEntity>())).ReturnsAsync(new List<string>());

        var result = await _sut.GetUsersByAttributeAsync("dept", "IT");

        result.Should().HaveCount(1);
    }

    #endregion
}
