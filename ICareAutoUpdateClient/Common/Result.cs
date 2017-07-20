namespace ICareAutoUpdateClient.Common
{
    public class Result
    {
        public Result(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public Result(bool isSuccess, long errorCode, string message)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            Message = message;
        }

        public Result(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static Result Success()
        {
            return new Result(true);
        }

        public static Result Fail(long errorCode, string message)
        {
            return new Result(false, errorCode, message);
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public bool IsSuccess { get; set; }

        public long ErrorCode { get; set; }

        public string Message { get; set; }
    }

    public class Result<T>
    {
        public Result(bool isSuccess, T value)
        {
            IsSuccess = isSuccess;
            Value = value;
        }

        public Result(bool isSuccess, long errorCode, string message, T value)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            Message = message;
            Value = value;
        }

        public Result(bool isSuccess, string message, T value)
        {
            IsSuccess = isSuccess;
            Message = message;
            Value = value;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value);
        }

        public static Result<T> Fail(long errorCode, string message, T value)
        {
            return new Result<T>(false, errorCode, message, value);
        }

        public static Result<T> Fail(string message, T value)
        {
            return new Result<T>(false, message, value);
        }

        public bool IsSuccess { get; set; }

        public long ErrorCode { get; set; }

        public string Message { get; set; }
        public T Value { get; set; }
    }
}