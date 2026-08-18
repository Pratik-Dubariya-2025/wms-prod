namespace WMS.Application.Common.Models
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public IDictionary<string, string[]>? Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string? message = null, int statusCode = 200)
        {
            Succeeded = true;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        public ApiResponse(string message, bool succeeded = false, int statusCode = 400)
        {
            Succeeded = succeeded;
            Message = message;
            StatusCode = statusCode;
        }

        public static ApiResponse<T> Success(T data, string? message = null, int statusCode = 200) => 
            new(data, message, statusCode);

        public static ApiResponse<T> Failure(string message, IDictionary<string, string[]>? errors = null, int statusCode = 400) =>
            new() { Succeeded = false, Message = message, Errors = errors, StatusCode = statusCode };
    }

    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse() : base()
        {
        }

        public ApiResponse(object data, string? message = null, int statusCode = 200) 
            : base(data, message, statusCode)
        {
        }

        public ApiResponse(string message, bool succeeded = false, int statusCode = 400) 
            : base(message, succeeded, statusCode)
        {
        }

        public static ApiResponse Success(string? message = null, int statusCode = 200) => 
            new(new object(), message, statusCode);

        public static new ApiResponse Failure(string message, IDictionary<string, string[]>? errors = null, int statusCode = 400) =>
            new() { Succeeded = false, Message = message, Errors = errors, StatusCode = statusCode };
    }
}
