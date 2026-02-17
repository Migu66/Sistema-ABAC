using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;
using Sistema.ABAC.Application.Common.Exceptions;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.Services;
using Sistema.ABAC.Domain.Entities;
using AutoMapper;
using Sistema.ABAC.Application.Mappings;
using System.Security.Claims;

namespace Sistema.ABAC.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly IMapper _mapper;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _jwtServiceMock = new Mock<IJwtService>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new AuthService(
            _userManagerMock.Object,
            _mapper,
            _jwtServiceMock.Object);
    }

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_ValidData_ReturnsToken()
    {
        var dto = new RegisterDto
        {
            UserName = "newuser",
            Email = "new@test.com",
            Password = "Pass123!",
            ConfirmPassword = "Pass123!",
            FullName = "Test User"
        };

        _userManagerMock.Setup(m => m.FindByNameAsync(dto.UserName))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), UserName = "newuser", Email = "new@test.com", FirstName = "Test", LastName = "User" });
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string>());
        _userManagerMock.Setup(m => m.GetClaimsAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<Claim>());

        _jwtServiceMock.Setup(j => j.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<IList<string>>(), It.IsAny<IList<Claim>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "jwt-token",
                TokenType = "Bearer",
                ExpiresIn = 3600,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RefreshToken = "refresh"
            });

        var result = await _sut.RegisterAsync(dto);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt-token");
    }

    [Fact]
    public async Task RegisterAsync_UsernameExists_ThrowsValidationException()
    {
        var dto = new RegisterDto { UserName = "existing", Email = "new@test.com", Password = "Pass123!" };

        _userManagerMock.Setup(m => m.FindByNameAsync(dto.UserName))
            .ReturnsAsync(new User { UserName = "existing" });

        var act = async () => await _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterAsync_EmailExists_ThrowsValidationException()
    {
        var dto = new RegisterDto { UserName = "newuser", Email = "existing@test.com", Password = "Pass123!" };

        _userManagerMock.Setup(m => m.FindByNameAsync(dto.UserName))
            .ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new User { Email = "existing@test.com" });

        var act = async () => await _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterAsync_IdentityFails_ThrowsValidationException()
    {
        var dto = new RegisterDto { UserName = "user", Email = "e@t.com", Password = "weak" };

        _userManagerMock.Setup(m => m.FindByNameAsync(dto.UserName)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((User?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordTooShort", Description = "Password too short" }));

        var act = async () => await _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var dto = new LoginDto { UserName = "user", Password = "Pass123!" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "user",
            Email = "e@t.com",
            FirstName = "F",
            LastName = "L",
            IsDeleted = false
        };

        _userManagerMock.Setup(m => m.FindByNameAsync(dto.UserName)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
        _userManagerMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());

        _jwtServiceMock.Setup(j => j.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<IList<string>>(), It.IsAny<IList<Claim>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "jwt",
                TokenType = "Bearer",
                ExpiresIn = 3600,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RefreshToken = "ref"
            });

        var result = await _sut.LoginAsync(dto);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt");
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsValidationException()
    {
        var dto = new LoginDto { UserName = "missing", Password = "p" };
        _userManagerMock.Setup(m => m.FindByNameAsync(dto.UserName)).ReturnsAsync((User?)null);

        var act = async () => await _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_UserDeleted_ThrowsValidationException()
    {
        var dto = new LoginDto { UserName = "deleted", Password = "p" };
        _userManagerMock.Setup(m => m.FindByNameAsync(dto.UserName))
            .ReturnsAsync(new User { UserName = "deleted", IsDeleted = true });

        var act = async () => await _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsValidationException()
    {
        var dto = new LoginDto { UserName = "user", Password = "wrong" };
        var user = new User { UserName = "user", IsDeleted = false };

        _userManagerMock.Setup(m => m.FindByNameAsync(dto.UserName)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

        var act = async () => await _sut.LoginAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region GenerateTokenAsync

    [Fact]
    public async Task GenerateTokenAsync_ValidUser_ReturnsToken()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            UserName = "u",
            Email = "e@t.com",
            FirstName = "F",
            LastName = "L",
            IsDeleted = false
        };

        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());
        _userManagerMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());

        _jwtServiceMock.Setup(j => j.GenerateTokenAsync(user, It.IsAny<IList<string>>(), It.IsAny<IList<Claim>>()))
            .ReturnsAsync(new TokenDto
            {
                AccessToken = "tok",
                TokenType = "Bearer",
                ExpiresIn = 3600,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                RefreshToken = "ref"
            });

        var result = await _sut.GenerateTokenAsync(userId);

        result.AccessToken.Should().Be("tok");
    }

    [Fact]
    public async Task GenerateTokenAsync_UserNotFound_ThrowsNotFoundException()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var act = async () => await _sut.GenerateTokenAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GenerateTokenAsync_DeletedUser_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(new User { Id = userId, IsDeleted = true });

        var act = async () => await _sut.GenerateTokenAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region RefreshTokenAsync

    [Fact]
    public async Task RefreshTokenAsync_EmptyAccessToken_ThrowsValidationException()
    {
        var dto = new RefreshTokenDto { AccessToken = "", RefreshToken = "ref" };

        var act = async () => await _sut.RefreshTokenAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_EmptyRefreshToken_ThrowsValidationException()
    {
        var dto = new RefreshTokenDto { AccessToken = "tok", RefreshToken = "" };

        var act = async () => await _sut.RefreshTokenAsync(dto);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidTokens_ThrowsNotImplementedException()
    {
        var dto = new RefreshTokenDto { AccessToken = "tok", RefreshToken = "ref" };

        var act = async () => await _sut.RefreshTokenAsync(dto);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    #endregion
}
