using Captcha.Configuration;
using Captcha.ExceptionHandling;
using Captcha.Interfaces;
using Captcha.Models;

namespace Captcha.Repository
{
    public class CaptchaService : ICaptchaService
    {
        private readonly ICaptchaTextGeneratorService _textGeneratorService;

        private readonly ICaptchaImageGeneratorService _imageGeneratorService;

        private readonly ICaptchaStore _captchaStore;

        private readonly IValidateCaptchaService _validateCaptchaService;

        private readonly ILogger<CaptchaService> _logger;

        private readonly CaptchaOptions _options;

        private readonly ICaptchaHashService _captchaHashService;

        public CaptchaService(
            ICaptchaTextGeneratorService textGeneratorService,
            ICaptchaImageGeneratorService imageGeneratorService,
            ICaptchaStore captchaStore,
            IValidateCaptchaService validateCaptchaService,
            ILogger<CaptchaService> logger,
            CaptchaOptions options, 
            ICaptchaHashService captchaHashService)
        {
            _textGeneratorService = textGeneratorService;
            _imageGeneratorService = imageGeneratorService;
            _captchaStore = captchaStore;
            _validateCaptchaService = validateCaptchaService;
            _logger = logger;
            this._options = options;
            _captchaHashService = captchaHashService;
        }

        
        public string CreateCaptcha()
        {
            var token = Guid.NewGuid().ToString("N");

            _logger.LogInformation("Captcha created with token {Token}", token);

            var captchaValue = _textGeneratorService.GenerateCaptchaText();

            var captchaData = new CaptchaData
            {
                CaptchaValue = captchaValue,
                CaptchaHashValue = _captchaHashService.Hash(captchaValue),
                CaptchaCreatedAt = DateTime.UtcNow
            };

            _captchaStore.Store(token, captchaData, TimeSpan.FromMinutes(_options.ExpirationMinutes));

            return token;
        }

        public byte[] GetCaptchaImage(string token)
        {
            _logger.LogDebug("Captcha image requested for token {Token}", token);

            var data = _captchaStore.Get(token);
            if (data is null)
                throw new CaptchaNotFoundException();

            byte[] captchaImg = _imageGeneratorService.GenerateCaptchaImage(data.CaptchaValue!);
            data.CaptchaValue = string.Empty;

            return captchaImg;
        }

        public CaptchaValidationResult ValidateCaptcha(CaptchaValidateRequest data)
        {
            return _validateCaptchaService.ValidateCaptcha(data);
        }
    }
}
