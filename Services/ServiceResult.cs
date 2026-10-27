namespace InvoiceManagementApi.Services;

public enum ResultKind
{
    Ok,
    NotFound,
    Conflict
}

/// <summary>
/// Small result object so services can report "not found" or "conflict"
/// without throwing exceptions or knowing anything about HTTP.
/// </summary>
public record ServiceResult<T>(ResultKind Kind, T? Value = default, string? Message = null)
{
    public static ServiceResult<T> Ok(T value) => new(ResultKind.Ok, value);
    public static ServiceResult<T> NotFound(string message) => new(ResultKind.NotFound, default, message);
    public static ServiceResult<T> Conflict(string message) => new(ResultKind.Conflict, default, message);
}
