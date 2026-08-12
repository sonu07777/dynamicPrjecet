namespace BikeShowroomAPI.Services;

public enum ServiceResultStatus
{
    Ok,
    NotFound,
    Forbidden,
    BadRequest
}

/// <summary>Carries the outcome of an operation that can fail, without throwing.</summary>
public class ServiceResult
{
    public bool Success { get; init; }
    public ServiceResultStatus Status { get; init; } = ServiceResultStatus.Ok;
    public string? Error { get; init; }

    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(string error, ServiceResultStatus status = ServiceResultStatus.BadRequest)
        => new() { Status = status, Error = error };
}

/// <summary>Carries the outcome of an operation that returns data.</summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static new ServiceResult<T> Fail(string error, ServiceResultStatus status = ServiceResultStatus.BadRequest)
        => new() { Status = status, Error = error };
}
