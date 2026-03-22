namespace Captcha.ExceptionHandling
{
    public class CaptchaExpiredException : DomainException
    {
        public CaptchaExpiredException() : base("Captcha has expired.") { }
    }
}
