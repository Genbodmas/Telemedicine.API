using System.Text.Json.Serialization;

namespace Telemedicine.API.Models
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> Success(string message, T data, int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Succeeded = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> Fail(string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Succeeded = false,
                Message = message,
                StatusCode = statusCode
            };
        }
    }
}
