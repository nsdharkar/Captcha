namespace Captcha.Interfaces
{
    public interface ICaptchaHashService
    {
        string Hash(string value);
        bool Verify(string value, string hash);
    }
}
