using System.Diagnostics.CodeAnalysis;

namespace Ambio.Application.Common;

public record ServiceResult
{
    public bool IsSuccess { get; }
    protected string? Message { get; }

    protected ServiceResult(bool isSuccess, string? message = null)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static ServiceResult Success() => new(true);
    public static ServiceResult Failure(string message) => new(false, message);

    public string GetMessage => Message ?? (IsSuccess ? "Operation Completed" : "An unknown error occurred");
}

public record ServiceResult<T> : ServiceResult
{
    private T? Value { get; }

    protected ServiceResult(bool isSuccess, T? value, string? message = null) : base(isSuccess, message)
    {
        Value = value;
    }

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        if (IsSuccess && Value is not null)
        {
            value = Value;
            return true;
        }
        value = default;
        return false;
    }

    public static ServiceResult<T> Success(T value) => new(true, value);
    public static new ServiceResult<T> Failure(string message) => new(false, default, message);
}


