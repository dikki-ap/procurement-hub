namespace ProcureHub.SharedKernel.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
    public string Timestamp { get; init; } = DateTime.UtcNow.ToString("o");

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, Dictionary<string, string[]>? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}

public static class ApiResponse
{
    public static ApiResponse<object?> Ok(string? message = null)
        => new() { Success = true, Message = message };

    public static ApiResponse<T> Ok<T>(T data, string? message = null)
        => ApiResponse<T>.Ok(data, message);

    public static ApiResponse<T> Fail<T>(string message, Dictionary<string, string[]>? errors = null)
        => ApiResponse<T>.Fail(message, errors);
}
