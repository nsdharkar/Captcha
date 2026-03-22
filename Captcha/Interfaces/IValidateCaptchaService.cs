using Captcha.Models;

namespace Captcha.Interfaces
{
    public interface IValidateCaptchaService
    {
        CaptchaValidationResult ValidateCaptcha(CaptchaValidateRequest request);
    }
}
