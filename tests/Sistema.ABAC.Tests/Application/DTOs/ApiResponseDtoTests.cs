using Sistema.ABAC.Application.DTOs.Common;

namespace Sistema.ABAC.Tests.Application.DTOs;

public class ApiResponseDtoTests
{
    [Fact]
    public void SuccessResponse_SetsCorrectValues()
    {
        var data = new { Name = "Test" };
        var result = ApiResponseDto<object>.SuccessResponse(data, "Custom message");

        Assert.True(result.Success);
        Assert.Equal("Custom message", result.Message);
        Assert.Equal(data, result.Data);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void SuccessResponse_DefaultMessage_SetsOperacionExitosa()
    {
        var result = ApiResponseDto<string>.SuccessResponse("data");

        Assert.True(result.Success);
        Assert.Equal("Operación exitosa", result.Message);
    }

    [Fact]
    public void ErrorResponse_SetsCorrectValues()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var result = ApiResponseDto<string>.ErrorResponse("Fail", errors);

        Assert.False(result.Success);
        Assert.Equal("Fail", result.Message);
        Assert.Equal(2, result.Errors.Count);
        Assert.Null(result.Data);
    }

    [Fact]
    public void ErrorResponse_NullErrors_DefaultsToEmptyList()
    {
        var result = ApiResponseDto<string>.ErrorResponse("Fail");

        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Timestamp_IsSetToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = new ApiResponseDto<string>();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(result.Timestamp, before, after);
    }
}
