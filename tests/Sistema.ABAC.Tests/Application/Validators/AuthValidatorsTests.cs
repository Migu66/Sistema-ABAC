using FluentAssertions;
using FluentValidation.TestHelper;
using Sistema.ABAC.Application.DTOs.Auth;
using Sistema.ABAC.Application.DTOs.Auth.Validators;

namespace Sistema.ABAC.Tests.Application.Validators;

#region Login Validator

public class LoginDtoValidatorTests
{
    private readonly LoginDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new LoginDto { UserName = "admin", Password = "password123" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserName_HasError()
    {
        var dto = new LoginDto { UserName = "", Password = "password123" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_ShortUserName_HasError()
    {
        var dto = new LoginDto { UserName = "ab", Password = "password123" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_EmptyPassword_HasError()
    {
        var dto = new LoginDto { UserName = "admin", Password = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_ShortPassword_HasError()
    {
        var dto = new LoginDto { UserName = "admin", Password = "12345" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_UserNameTooLong_HasError()
    {
        var dto = new LoginDto { UserName = new string('a', 101), Password = "password123" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }
}

#endregion

#region Register Validator

public class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new RegisterDto
        {
            UserName = "newuser",
            Email = "new@test.com",
            Password = "Pass123@!",
            ConfirmPassword = "Pass123@!",
            FullName = "New User"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserName_HasError()
    {
        var dto = new RegisterDto { UserName = "", Email = "a@b.com", Password = "Pass123@!", ConfirmPassword = "Pass123@!", FullName = "Test" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_InvalidUserNameChars_HasError()
    {
        var dto = new RegisterDto { UserName = "user name!", Email = "a@b.com", Password = "Pass123@!", ConfirmPassword = "Pass123@!", FullName = "Test" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_InvalidEmail_HasError()
    {
        var dto = new RegisterDto { UserName = "user", Email = "not-an-email", Password = "Pass123@!", ConfirmPassword = "Pass123@!", FullName = "Test" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WeakPassword_HasError()
    {
        var dto = new RegisterDto { UserName = "user", Email = "a@b.com", Password = "weakpass", ConfirmPassword = "weakpass", FullName = "Test" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_MismatchedPasswords_HasError()
    {
        var dto = new RegisterDto { UserName = "user", Email = "a@b.com", Password = "Pass123@!", ConfirmPassword = "Different1@", FullName = "Test" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Validate_EmptyFullName_HasError()
    {
        var dto = new RegisterDto { UserName = "user", Email = "a@b.com", Password = "Pass123@!", ConfirmPassword = "Pass123@!", FullName = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_InvalidPhoneNumber_HasError()
    {
        var dto = new RegisterDto { UserName = "user", Email = "a@b.com", Password = "Pass123@!", ConfirmPassword = "Pass123@!", FullName = "Test User", PhoneNumber = "not-a-phone" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }
}

#endregion

#region ChangePassword Validator

public class ChangePasswordDtoValidatorTests
{
    private readonly ChangePasswordDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new ChangePasswordDto { CurrentPassword = "OldPass1!", NewPassword = "NewPass1@", ConfirmNewPassword = "NewPass1@" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyCurrentPassword_HasError()
    {
        var dto = new ChangePasswordDto { CurrentPassword = "", NewPassword = "NewPass1@", ConfirmNewPassword = "NewPass1@" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Validate_WeakNewPassword_HasError()
    {
        var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "weak", ConfirmNewPassword = "weak" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_MismatchedConfirmation_HasError()
    {
        var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "NewPass1@", ConfirmNewPassword = "Different1@" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }

    [Fact]
    public void Validate_PasswordWithoutUppercase_HasError()
    {
        var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "newpass1@", ConfirmNewPassword = "newpass1@" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_PasswordWithoutNumber_HasError()
    {
        var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "NewPasss@", ConfirmNewPassword = "NewPasss@" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_PasswordWithoutSpecialChar_HasError()
    {
        var dto = new ChangePasswordDto { CurrentPassword = "old", NewPassword = "NewPass12", ConfirmNewPassword = "NewPass12" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}

#endregion

#region ForgotPassword Validator

public class ForgotPasswordDtoValidatorTests
{
    private readonly ForgotPasswordDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidEmail_HasNoErrors()
    {
        var dto = new ForgotPasswordDto { Email = "user@example.com" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_HasError()
    {
        var dto = new ForgotPasswordDto { Email = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_InvalidEmail_HasError()
    {
        var dto = new ForgotPasswordDto { Email = "not-an-email" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmailTooLong_HasError()
    {
        var dto = new ForgotPasswordDto { Email = new string('a', 257) + "@b.com" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}

#endregion

#region RefreshToken Validator

public class RefreshTokenDtoValidatorTests
{
    private readonly RefreshTokenDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidTokens_HasNoErrors()
    {
        var dto = new RefreshTokenDto { AccessToken = new string('a', 25), RefreshToken = new string('b', 25) };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyAccessToken_HasError()
    {
        var dto = new RefreshTokenDto { AccessToken = "", RefreshToken = new string('b', 25) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AccessToken);
    }

    [Fact]
    public void Validate_ShortAccessToken_HasError()
    {
        var dto = new RefreshTokenDto { AccessToken = "short", RefreshToken = new string('b', 25) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AccessToken);
    }

    [Fact]
    public void Validate_EmptyRefreshToken_HasError()
    {
        var dto = new RefreshTokenDto { AccessToken = new string('a', 25), RefreshToken = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}

#endregion

#region ResetPassword Validator

public class ResetPasswordDtoValidatorTests
{
    private readonly ResetPasswordDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new ResetPasswordDto
        {
            Email = "user@example.com",
            Token = "valid-reset-token",
            NewPassword = "NewPass1@",
            ConfirmNewPassword = "NewPass1@"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_HasError()
    {
        var dto = new ResetPasswordDto { Email = "", Token = "t", NewPassword = "NewPass1@", ConfirmNewPassword = "NewPass1@" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmptyToken_HasError()
    {
        var dto = new ResetPasswordDto { Email = "a@b.com", Token = "", NewPassword = "NewPass1@", ConfirmNewPassword = "NewPass1@" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Validate_WeakPassword_HasError()
    {
        var dto = new ResetPasswordDto { Email = "a@b.com", Token = "t", NewPassword = "weak", ConfirmNewPassword = "weak" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_MismatchedPasswords_HasError()
    {
        var dto = new ResetPasswordDto { Email = "a@b.com", Token = "t", NewPassword = "NewPass1@", ConfirmNewPassword = "Other1@!" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }
}

#endregion

#region UpdateUser Validator

public class UpdateUserDtoValidatorTests
{
    private readonly UpdateUserDtoValidator _validator = new();

    [Fact]
    public void Validate_EmptyDto_HasNoErrors()
    {
        var dto = new UpdateUserDto();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidFullName_HasNoError()
    {
        var dto = new UpdateUserDto { FullName = "John Doe" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_FullNameTooLong_HasError()
    {
        var dto = new UpdateUserDto { FullName = new string('a', 201) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_InvalidEmail_HasError()
    {
        var dto = new UpdateUserDto { Email = "not-email" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_InvalidPhone_HasError()
    {
        var dto = new UpdateUserDto { PhoneNumber = "abc-not-phone" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_DepartmentTooLong_HasError()
    {
        var dto = new UpdateUserDto { Department = new string('a', 101) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Department);
    }
}

#endregion
