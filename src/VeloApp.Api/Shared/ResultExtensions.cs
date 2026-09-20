namespace VeloApp.Api.Shared;

/// <summary>
/// 将 <see cref="Result"/> / <see cref="Result{T}"/> 映射为合适的 HTTP 状态码。
/// </summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult(
        this Result result,
        int successStatusCode = StatusCodes.Status200OK,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        if (!result.IsSuccess)
            return Results.Json(result, statusCode: failureStatusCode);

        return successStatusCode == StatusCodes.Status204NoContent
            ? Results.NoContent()
            : Results.Json(result, statusCode: successStatusCode);
    }

    public static IResult ToHttpResult<T>(
        this Result<T> result,
        int successStatusCode = StatusCodes.Status200OK,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        if (!result.IsSuccess)
            return Results.Json(result, statusCode: failureStatusCode);

        return successStatusCode == StatusCodes.Status204NoContent
            ? Results.NoContent()
            : Results.Json(result, statusCode: successStatusCode);
    }

    public static async Task<IResult> ToHttpResultAsync(
        this Task<Result> resultTask,
        int successStatusCode = StatusCodes.Status200OK,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        var result = await resultTask;
        return result.ToHttpResult(successStatusCode, failureStatusCode);
    }

    public static async Task<IResult> ToHttpResultAsync<T>(
        this Task<Result<T>> resultTask,
        int successStatusCode = StatusCodes.Status200OK,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        var result = await resultTask;
        return result.ToHttpResult(successStatusCode, failureStatusCode);
    }
}
