namespace Captcha.Models
{
    public class CaptchaValidationResult
    {
        public bool IsSuccess { get; }

        public string? ErrorMessage { get; }

        public CaptchaValidationResult(bool success, string? message = null)
        {
            IsSuccess = success;
            ErrorMessage = message;
        }

        public static CaptchaValidationResult Success()
            => new(true);

        public static CaptchaValidationResult Failure(string message)
            => new(false, message);
    }
}
