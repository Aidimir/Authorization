namespace Domain.Models;

public class Result<T>
{
    public bool Success { get; set; }
    public T? Value { get; set; }
    public string? ErrorDescription { get; set; }

    public static Result<T> Ok(T value)
    {
        return new Result<T> {Success = true, Value = value};
    }

    public static Result<T> Fail(string errorDescription)
    {
        return new Result<T> {Success = false, ErrorDescription = errorDescription};
    }
}