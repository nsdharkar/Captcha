namespace Captcha.ExceptionHandling
{
    public class TokenRequiredException : DomainException
    {
        public TokenRequiredException(string message)
            : base(message)
        {
        }
    }
}
