namespace Captcha.Repository
{
    public interface ICaptchaTextGeneratorService
    {
        string GenerateCaptchaText(int capLength=6);
    }
}
