namespace Engine.Helpers;

using System.Collections;

public abstract class Result
{
    public bool Success { get; protected set; }
    public bool Failure => !Success;

    public static Result<T> Successful<T>(T data) => new SuccessResult<T>(data);

    public static Result Successful() => new SuccessResult();

    public static Result<T> Error<T>(string message) => new ErrorResult<T>(message);

    public static Result Error(string message) => new ErrorResult(message);

    public TR Map<TR>(Func<TR> success, Func<IErrorResult, TR> error)
    {
	    if (Success)
	    {
		    return success();
	    }
	    else
	    {
		    return error((IErrorResult)this);
	    }
    }

    public Result MapError(Func<IErrorResult, Result> error)
    {
	    if (Success)
	    {
		    return Result.Successful();
	    }
	    else
	    {
		    return error((IErrorResult)this);
	    }
    }
}

public abstract class Result<T> : Result, IEnumerable<T>
{
    private T _data;

    protected Result(T data) => Data = data;

    public T Data
    {
        get =>
            Success
                ? _data
                : throw new Exception(
                    $"You can't access .{nameof(Data)} when .{nameof(Success)} is false. Error Message:{(this as IErrorResult)?.Message}");
        set => _data = value;
    }

    public TR Map<TR>(Func<T, TR> success, Func<IErrorResult, TR> error)
    {
	    if (Success)
	    {
		    return success(Data);
	    }
	    else
	    {
		    return error((IErrorResult)this);
	    }
    }

    // public TR FlatMap<TR>(Func<T, Result<TR>> success, Func<IErrorResult, Result<TR>> error)
    // {
	   //  if (Success)
	   //  {
		  //   return success(Data);
	   //  }
	   //  else
	   //  {
		  //   return error((IErrorResult)this);
	   //  }
    // }

    public IEnumerator<T> GetEnumerator()
    {
        if (Success)
        {
            return new List<T> {_data}.GetEnumerator();
        }

        return new List<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class SuccessResult : Result
{
    public SuccessResult() => Success = true;
}

public class SuccessResult<T> : Result<T>
{
    public SuccessResult(T data) : base(data) => Success = true;
}

public class ErrorResult : Result, IErrorResult
{
    public ErrorResult(string message) : this(message, Array.Empty<Error>())
    {
    }

    public ErrorResult(string message, IReadOnlyCollection<Error> errors)
    {
        Message = message;
        Success = false;
        Errors = errors ?? Array.Empty<Error>();
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
        Message = message;
        Success = false;
        Errors = errors ?? Array.Empty<Error>();
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
        Code = code;
        Details = details;
    }

    public string Code { get; }
    public string Details { get; }
}

public interface IErrorResult
{
    string Message { get; }
    IReadOnlyCollection<Error> Errors { get; }
}
