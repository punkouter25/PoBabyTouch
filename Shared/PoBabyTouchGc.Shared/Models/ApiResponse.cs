namespace PoBabyTouchGc.Shared.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}
