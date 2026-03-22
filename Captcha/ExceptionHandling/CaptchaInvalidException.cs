namespace Captcha.ExceptionHandling
{
    public class CaptchaInvalidException : DomainException
    {
        public CaptchaInvalidException() : base("Invalid Captcha.") { }
    }
}
