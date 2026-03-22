namespace Captcha.Interfaces
{
    public interface ICaptchaImageGeneratorService
    {
        byte[] GenerateCaptchaImage(string captchaText);
    }
}
