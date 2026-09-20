namespace VeloApp.Api.Shared;

/// <summary>
/// 无返回值的统一结果：封装成功/失败，避免到处抛异常表达业务失败。
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// 带返回值的统一结果：封装成功/失败，避免到处抛异常表达业务失败。
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// 允许直接 return value，隐式转为 Result&lt;T&gt;.Success(value)。
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}
