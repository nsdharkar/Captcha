using Captcha.Configuration;
using Captcha.Interfaces;
using Captcha.Models;

namespace Captcha.Repository
{
    public class ValidateCaptchaService : IValidateCaptchaService
    {
        private readonly ILogger<ValidateCaptchaService> _logger;
        private readonly ICaptchaStore _captchaStore;
        private readonly CaptchaOptions _options;
        private readonly ICaptchaHashService _captchaHashService;

        public ValidateCaptchaService(ILogger<ValidateCaptchaService> logger, ICaptchaStore captchaStore, CaptchaOptions options, 
            ICaptchaHashService captchaHashService)
        {
            _logger = logger;
            _captchaStore = captchaStore;
            this._options = options;
            _captchaHashService = captchaHashService;
        }

        public CaptchaValidationResult ValidateCaptcha(CaptchaValidateRequest data)
        {
            _logger.LogDebug("Starting Captcha Image Validation.");

            if (data is null)
            {
                return CaptchaValidationResult.Failure("Invalid Request");
            }

            var captchaData = _captchaStore.Get(data.Token);

            if (captchaData is null)
            {
                return CaptchaValidationResult.Failure("Captcha expired or invalid token");
            }

            if (DateTime.UtcNow - captchaData.CaptchaCreatedAt > TimeSpan.FromMinutes(_options.ExpirationMinutes))
            {
                _captchaStore.Remove(data.Token);
                return CaptchaValidationResult.Failure("Captcha expired");
            }

            if (!_captchaHashService.Verify(data.UserInput, captchaData.CaptchaHashValue))
            {
                _logger.LogWarning("Captcha validation failed for token {Token}",data.Token);

                return CaptchaValidationResult.Failure("Invalid Captcha");
            }

            _captchaStore.Remove(data.Token); // one-time use

            return CaptchaValidationResult.Success();

        }
    }
}
