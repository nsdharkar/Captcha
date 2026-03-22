namespace Captcha.Models
{
    public class CaptchaData
    {
        public string? CaptchaValue { get; set; }
        public string CaptchaHashValue { get; init; } = default!;
        public DateTime CaptchaCreatedAt { get; init; }
    }
}
