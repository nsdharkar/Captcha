namespace Captcha.Repository
{
    public interface ICaptchaImageGeneratorService
    {
        byte[] GenerateCaptchaImage(string captchaText);
    }
}
