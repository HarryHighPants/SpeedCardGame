namespace Engine.Helpers;

using System.Collections;

public abstract class Result
{
    public bool Success { get; protected set; }
    public bool Failure => !this.Success;

    public static Result<T> Successful<T>(T data) => new SuccessResult<T>(data);

    public static Result Successful() => new SuccessResult();

    public static Result<T> Error<T>(string message) => new ErrorResult<T>(message);

    public static Result Error(string message) => new ErrorResult(message);
}

public abstract class Result<T> : Result, IEnumerable<T>
{
    private T _data;

    protected Result(T data) => this.Data = data;

    public T Data
    {
        get =>
            this.Success
                ? this._data
                : throw new Exception(
                    $"You can't access .{nameof(this.Data)} when .{nameof(this.Success)} is false. Error Message:{(this as IErrorResult)?.Message}");
        set => this._data = value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (this.Success)
        {
            return new List<T> {this._data}.GetEnumerator();
        }

        return new List<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

public class SuccessResult : Result
{
    public SuccessResult() => this.Success = true;
}

public class SuccessResult<T> : Result<T>
{
    public SuccessResult(T data) : base(data) => this.Success = true;
}

public class ErrorResult : Result, IErrorResult
{
    public ErrorResult(string message) : this(message, Array.Empty<Error>())
    {
    }

    public ErrorResult(string message, IReadOnlyCollection<Error> errors)
    {
        this.Message = message;
        this.Success = false;
        this.Errors = errors ?? Array.Empty<Error>();
    }

    public string Message { get; }
    public IReadOnlyCollection<Error> Errors { get; }
}

public class ErrorResult<T> : Result<T>, IErrorResult
{
    public ErrorResult(string message) : this(message, Array.Empty<Error>())
    {
    }

    public ErrorResult(string message, IReadOnlyCollection<Error> errors) : base(default)
    {
        this.Message = message;
        this.Success = false;
        this.Errors = errors ?? Array.Empty<Error>();
    }

    public string Message { get; set; }
    public IReadOnlyCollection<Error> Errors { get; }
}

public class Error
{
    public Error(string details) : this(null, details)
    {
    }

    public Error(string code, string details)
    {
        this.Code = code;
        this.Details = details;
    }

    public string Code { get; }
    public string Details { get; }
}

public interface IErrorResult
{
    string Message { get; }
    IReadOnlyCollection<Error> Errors { get; }
}
