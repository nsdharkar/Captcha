namespace Captcha.Models
{
    public class CaptchaValidateRequest
    {
        public string Token { get; set; } = default!;
        public string UserInput { get; set; } = default!;
    }
}
