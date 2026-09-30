using Ambio.Application.Common;

namespace Ambio.Application.Tests.Common;

public class ServiceResultTests
{
    [Theory]
    [MemberData(nameof(SuccessValues))]
    public void Success_TryGetValue_ReturnsValueOfExpectedType<T>(T expected)
    {
        var result = ServiceResult<T>.Success(expected);

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var value));
        Assert.IsType<T>(value);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void Failure_TryGetValue_ReturnsFalse()
    {
        var result = ServiceResult<string>.Failure("error");

        Assert.False(result.IsSuccess);
        Assert.False(result.TryGetValue(out var value));
        Assert.Null(value);
    }

    [Fact]
    public void Failure_GetMessage_ReturnsMessage()
    {
        Assert.Equal("error", ServiceResult.Failure("error").GetMessage);
    }

    [Fact]
    public void Success_GetMessage_ReturnsCompletedMessage()
    {
        Assert.Equal("Operation Completed", ServiceResult.Success().GetMessage);
    }

    private sealed record SampleDto(int Id, string Name);

    public static IEnumerable<object[]> SuccessValues =>
    [
        ["value"],
        [42],
        [new SampleDto(1, "sample")],
    ];
}
