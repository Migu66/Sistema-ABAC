using FluentAssertions;
using Sistema.ABAC.Application.Common.Exceptions;
using ApplicationException = Sistema.ABAC.Application.Common.Exceptions.ApplicationException;

namespace Sistema.ABAC.Tests.Application.Exceptions;

public class ExceptionsTests
{
    #region BadRequestException

    [Fact]
    public void BadRequestException_WithMessage_SetsMessage()
    {
        var ex = new BadRequestException("Invalid input");

        ex.Message.Should().Be("Invalid input");
        ex.Should().BeAssignableTo<ApplicationException>();
    }

    [Fact]
    public void BadRequestException_WithParameterAndReason_FormatsMessage()
    {
        var ex = new BadRequestException("userId", "must be a valid GUID");

        ex.Message.Should().Contain("userId");
        ex.Message.Should().Contain("must be a valid GUID");
    }

    #endregion

    #region ConflictException

    [Fact]
    public void ConflictException_WithMessage_SetsMessage()
    {
        var ex = new ConflictException("Conflict occurred");

        ex.Message.Should().Be("Conflict occurred");
        ex.Should().BeAssignableTo<ApplicationException>();
    }

    [Fact]
    public void ConflictException_WithEntityAndKey_FormatsMessage()
    {
        var ex = new ConflictException("Action", "read");

        ex.Message.Should().Contain("Action");
        ex.Message.Should().Contain("read");
    }

    #endregion

    #region ForbiddenAccessException

    [Fact]
    public void ForbiddenAccessException_Default_SetsDefaultMessage()
    {
        var ex = new ForbiddenAccessException();

        ex.Message.Should().NotBeNullOrEmpty();
        ex.Should().BeAssignableTo<ApplicationException>();
    }

    [Fact]
    public void ForbiddenAccessException_WithMessage_SetsCustomMessage()
    {
        var ex = new ForbiddenAccessException("Custom forbidden");

        ex.Message.Should().Be("Custom forbidden");
    }

    #endregion

    #region InternalServerErrorException

    [Fact]
    public void InternalServerErrorException_WithMessage_SetsMessage()
    {
        var ex = new InternalServerErrorException("Server error");

        ex.Message.Should().Be("Server error");
        ex.Should().BeAssignableTo<ApplicationException>();
    }

    [Fact]
    public void InternalServerErrorException_WithMessageAndInner_SetsInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new InternalServerErrorException("Server error", inner);

        ex.Message.Should().Be("Server error");
        ex.InnerException.Should().Be(inner);
    }

    #endregion

    #region NotFoundException

    [Fact]
    public void NotFoundException_WithEntityAndKey_FormatsMessage()
    {
        var id = Guid.NewGuid();
        var ex = new NotFoundException("Policy", id);

        ex.Message.Should().Contain("Policy");
        ex.Message.Should().Contain(id.ToString());
        ex.Should().BeAssignableTo<ApplicationException>();
    }

    [Fact]
    public void NotFoundException_WithMessage_SetsMessage()
    {
        var ex = new NotFoundException("Not found");

        ex.Message.Should().Be("Not found");
    }

    #endregion

    #region UnauthorizedException

    [Fact]
    public void UnauthorizedException_Default_SetsDefaultMessage()
    {
        var ex = new UnauthorizedException();

        ex.Message.Should().NotBeNullOrEmpty();
        ex.Should().BeAssignableTo<ApplicationException>();
    }

    [Fact]
    public void UnauthorizedException_WithMessage_SetsCustomMessage()
    {
        var ex = new UnauthorizedException("Custom unauthorized");

        ex.Message.Should().Be("Custom unauthorized");
    }

    #endregion

    #region ValidationException

    [Fact]
    public void ValidationException_Default_SetsDefaultMessage()
    {
        var ex = new ValidationException();

        ex.Message.Should().NotBeNullOrEmpty();
        ex.Errors.Should().BeEmpty();
        ex.Should().BeAssignableTo<ApplicationException>();
    }

    [Fact]
    public void ValidationException_WithErrors_SetsErrorsDictionary()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Required" } },
            { "Email", new[] { "Invalid format", "Already taken" } }
        };

        var ex = new ValidationException(errors);

        ex.Errors.Should().HaveCount(2);
        ex.Errors["Name"].Should().Contain("Required");
        ex.Errors["Email"].Should().HaveCount(2);
    }

    [Fact]
    public void ValidationException_WithPropertyAndMessage_CreatesErrorEntry()
    {
        var ex = new ValidationException("Code", "Code already exists");

        ex.Errors.Should().ContainKey("Code");
        ex.Errors["Code"].Should().Contain("Code already exists");
    }

    #endregion
}
