namespace Captcha.ExceptionHandling
{
    public class CaptchaGenerationException : DomainException
    {
        public CaptchaGenerationException(string message)
            : base(message)
        {
        }

        public CaptchaGenerationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
