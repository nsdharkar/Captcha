using Captcha.Models;

namespace Captcha.Interfaces
{
    public interface ICaptchaService
    {
        string CreateCaptcha();
        byte[] GetCaptchaImage(string token);
        CaptchaValidationResult ValidateCaptcha(CaptchaValidateRequest data);
    }
}
