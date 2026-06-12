namespace AutoNex.Helpers;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Operación exitosa")
        => new() { Data = data, Success = true, Message = message };

    public static ApiResponse<T> Fail(string message, Dictionary<string, string[]>? errors = null)
        => new() { Data = default, Success = false, Message = message, Errors = errors };
}
