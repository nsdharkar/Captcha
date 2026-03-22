namespace Captcha.ExceptionHandling
{
    public class CaptchaNotFoundException : DomainException
    {
        public CaptchaNotFoundException() : base("Captcha token not found.") { }
    }
}
