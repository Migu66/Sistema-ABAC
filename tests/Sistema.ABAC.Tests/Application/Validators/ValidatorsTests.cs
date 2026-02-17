using FluentAssertions;
using FluentValidation.TestHelper;
using Sistema.ABAC.Application.DTOs;
using Sistema.ABAC.Application.DTOs.Validators;
using Sistema.ABAC.Domain.Enums;

namespace Sistema.ABAC.Tests.Application.Validators;

#region Action Validators

public class CreateActionDtoValidatorTests
{
    private readonly CreateActionDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new CreateActionDto { Name = "Read", Code = "read", Description = "Read action" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var dto = new CreateActionDto { Name = "", Code = "read" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyCode_HasError()
    {
        var dto = new CreateActionDto { Name = "Read", Code = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_InvalidCodeFormat_HasError()
    {
        var dto = new CreateActionDto { Name = "Read", Code = "InvalidCode!" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_NameWithHtml_HasError()
    {
        var dto = new CreateActionDto { Name = "<script>alert('xss')</script>", Code = "read" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_HasError()
    {
        var dto = new CreateActionDto { Name = new string('a', 101), Code = "read" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_CodeTooLong_HasError()
    {
        var dto = new CreateActionDto { Name = "Read", Code = new string('a', 51) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_ValidSnakeCaseCode_NoError()
    {
        var dto = new CreateActionDto { Name = "Read All", Code = "read_all_items" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Code);
    }
}

public class UpdateActionDtoValidatorTests
{
    private readonly UpdateActionDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new UpdateActionDto { Name = "Updated" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var dto = new UpdateActionDto { Name = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_DescriptionWithHtml_HasError()
    {
        var dto = new UpdateActionDto { Name = "Valid", Description = "<b>bold</b>" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}

#endregion

#region Attribute Validators

public class CreateAttributeDtoValidatorTests
{
    private readonly CreateAttributeDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new CreateAttributeDto { Name = "Department", Key = "department", Type = AttributeType.String };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var dto = new CreateAttributeDto { Name = "", Key = "dept", Type = AttributeType.String };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyKey_HasError()
    {
        var dto = new CreateAttributeDto { Name = "Dept", Key = "", Type = AttributeType.String };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_InvalidKeyFormat_HasError()
    {
        var dto = new CreateAttributeDto { Name = "Dept", Key = "Invalid Key!", Type = AttributeType.String };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Key);
    }

    [Fact]
    public void Validate_InvalidType_HasError()
    {
        var dto = new CreateAttributeDto { Name = "Dept", Key = "dept", Type = (AttributeType)999 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }
}

public class UpdateAttributeDtoValidatorTests
{
    private readonly UpdateAttributeDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new UpdateAttributeDto { Name = "Updated" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var dto = new UpdateAttributeDto { Name = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

#endregion

#region Policy Validators

public class CreatePolicyDtoValidatorTests
{
    private readonly CreatePolicyDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new CreatePolicyDto { Name = "Admin Policy", Effect = PolicyEffect.Permit, Priority = 100 };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var dto = new CreatePolicyDto { Name = "", Effect = PolicyEffect.Permit };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_PriorityTooHigh_HasError()
    {
        var dto = new CreatePolicyDto { Name = "P", Effect = PolicyEffect.Permit, Priority = 1000 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Validate_NegativePriority_HasError()
    {
        var dto = new CreatePolicyDto { Name = "P", Effect = PolicyEffect.Permit, Priority = -1 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Validate_WithHtmlInName_HasError()
    {
        var dto = new CreatePolicyDto { Name = "<script>xss</script>", Effect = PolicyEffect.Permit };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithValidConditions_HasNoErrors()
    {
        var dto = new CreatePolicyDto
        {
            Name = "Policy",
            Effect = PolicyEffect.Permit,
            Priority = 50,
            Conditions = new List<CreatePolicyConditionDto>
            {
                new() { AttributeType = "Subject", AttributeKey = "dept", Operator = OperatorType.Equals, ExpectedValue = "IT" }
            }
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class UpdatePolicyDtoValidatorTests
{
    private readonly UpdatePolicyDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new UpdatePolicyDto { Name = "Updated", Effect = PolicyEffect.Deny, Priority = 200 };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var dto = new UpdatePolicyDto { Name = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

#endregion

#region PolicyCondition Validators

public class CreatePolicyConditionDtoValidatorTests
{
    private readonly CreatePolicyConditionDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new CreatePolicyConditionDto
        {
            AttributeType = "Subject",
            AttributeKey = "department",
            Operator = OperatorType.Equals,
            ExpectedValue = "IT"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyAttributeType_HasError()
    {
        var dto = new CreatePolicyConditionDto { AttributeType = "", AttributeKey = "key", Operator = OperatorType.Equals, ExpectedValue = "v" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AttributeType);
    }

    [Fact]
    public void Validate_EmptyExpectedValue_HasError()
    {
        var dto = new CreatePolicyConditionDto { AttributeType = "Subject", AttributeKey = "key", Operator = OperatorType.Equals, ExpectedValue = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ExpectedValue);
    }

    [Fact]
    public void Validate_HtmlInAttributeKey_HasError()
    {
        var dto = new CreatePolicyConditionDto { AttributeType = "Subject", AttributeKey = "<b>key</b>", Operator = OperatorType.Equals, ExpectedValue = "v" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AttributeKey);
    }
}

public class UpdatePolicyConditionDtoValidatorTests
{
    private readonly UpdatePolicyConditionDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new UpdatePolicyConditionDto
        {
            AttributeType = "Resource",
            AttributeKey = "type",
            Operator = OperatorType.Contains,
            ExpectedValue = "document"
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyFields_HasErrors()
    {
        var dto = new UpdatePolicyConditionDto { AttributeType = "", AttributeKey = "", ExpectedValue = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AttributeType);
        result.ShouldHaveValidationErrorFor(x => x.AttributeKey);
        result.ShouldHaveValidationErrorFor(x => x.ExpectedValue);
    }
}

#endregion

#region Resource Validators

public class CreateResourceDtoValidatorTests
{
    private readonly CreateResourceDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new CreateResourceDto { Name = "Document", Type = "File" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var dto = new CreateResourceDto { Name = "", Type = "File" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyType_HasError()
    {
        var dto = new CreateResourceDto { Name = "Document", Type = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Validate_NameTooLong_HasError()
    {
        var dto = new CreateResourceDto { Name = new string('x', 201), Type = "File" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

public class UpdateResourceDtoValidatorTests
{
    private readonly UpdateResourceDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new UpdateResourceDto { Name = "Updated", Type = "File" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_HtmlInName_HasError()
    {
        var dto = new UpdateResourceDto { Name = "<img src=x>", Type = "File" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

#endregion

#region User Attribute Validators

public class AssignUserAttributeDtoValidatorTests
{
    private readonly AssignUserAttributeDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = "IT" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyAttributeId_HasError()
    {
        var dto = new AssignUserAttributeDto { AttributeId = Guid.Empty, Value = "IT" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AttributeId);
    }

    [Fact]
    public void Validate_EmptyValue_HasError()
    {
        var dto = new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_ValueTooLong_HasError()
    {
        var dto = new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = new string('x', 501) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_ValidToBeforeValidFrom_HasError()
    {
        var dto = new AssignUserAttributeDto
        {
            AttributeId = Guid.NewGuid(),
            Value = "IT",
            ValidFrom = DateTime.UtcNow.AddDays(1),
            ValidTo = DateTime.UtcNow
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ValidTo);
    }

    [Fact]
    public void Validate_HtmlInValue_HasError()
    {
        var dto = new AssignUserAttributeDto { AttributeId = Guid.NewGuid(), Value = "<script>alert(1)</script>" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }
}

public class UpdateUserAttributeDtoValidatorTests
{
    private readonly UpdateUserAttributeDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new UpdateUserAttributeDto { Value = "Updated" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyValue_HasError()
    {
        var dto = new UpdateUserAttributeDto { Value = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_ValidToBeforeValidFrom_HasError()
    {
        var dto = new UpdateUserAttributeDto
        {
            Value = "v",
            ValidFrom = DateTime.UtcNow.AddDays(1),
            ValidTo = DateTime.UtcNow
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ValidTo);
    }
}

#endregion

#region Resource Attribute Validators

public class AssignResourceAttributeDtoValidatorTests
{
    private readonly AssignResourceAttributeDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidDto_HasNoErrors()
    {
        var dto = new AssignResourceAttributeDto { AttributeId = Guid.NewGuid(), Value = "Public" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyValue_HasError()
    {
        var dto = new AssignResourceAttributeDto { AttributeId = Guid.NewGuid(), Value = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public void Validate_EmptyAttributeId_HasError()
    {
        var dto = new AssignResourceAttributeDto { AttributeId = Guid.Empty, Value = "Public" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AttributeId);
    }
}

#endregion

#region AccessLogFilter Validator

public class AccessLogFilterDtoValidatorTests
{
    private readonly AccessLogFilterDtoValidator _validator = new();

    [Fact]
    public void Validate_DefaultFilter_HasNoErrors()
    {
        var dto = new AccessLogFilterDto();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_PageZero_HasError()
    {
        var dto = new AccessLogFilterDto { Page = 0 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_PageSizeTooLarge_HasError()
    {
        var dto = new AccessLogFilterDto { PageSize = 201 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_InvalidSortBy_HasError()
    {
        var dto = new AccessLogFilterDto { SortBy = "InvalidField" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void Validate_ValidSortBy_HasNoError()
    {
        var dto = new AccessLogFilterDto { SortBy = "Result" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void Validate_ToDateBeforeFromDate_HasError()
    {
        var dto = new AccessLogFilterDto
        {
            FromDate = DateTime.UtcNow,
            ToDate = DateTime.UtcNow.AddDays(-1)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ToDate);
    }
}

#endregion

#region EvaluateAccessRequest Validator

public class EvaluateAccessRequestValidatorTests
{
    private readonly EvaluateAccessRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var dto = new EvaluateAccessRequest { UserId = Guid.NewGuid(), ResourceId = Guid.NewGuid(), ActionId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_HasError()
    {
        var dto = new EvaluateAccessRequest { UserId = Guid.Empty, ResourceId = Guid.NewGuid(), ActionId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_EmptyResourceId_HasError()
    {
        var dto = new EvaluateAccessRequest { UserId = Guid.NewGuid(), ResourceId = Guid.Empty, ActionId = Guid.NewGuid() };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ResourceId);
    }

    [Fact]
    public void Validate_EmptyActionId_HasError()
    {
        var dto = new EvaluateAccessRequest { UserId = Guid.NewGuid(), ResourceId = Guid.NewGuid(), ActionId = Guid.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ActionId);
    }
}

#endregion
