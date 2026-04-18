using Captcha.Configuration;
using Captcha.ExceptionHandling;
using Captcha.Interfaces;
using Captcha.Models;

namespace Captcha.Repository
{
    /// <summary>
    /// Provides methods for creating, retrieving, and validating CAPTCHA challenges to help prevent automated
    /// submissions and ensure user authenticity.
    /// </summary>
    /// <remarks>This service coordinates the generation of CAPTCHA text, image creation, storage, and
    /// validation using injected dependencies. It is typically used in web applications to add human verification steps
    /// to forms or actions. Thread safety and expiration policies depend on the underlying store and configuration
    /// options.</remarks>
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

        /// <summary>
        /// Creates a new CAPTCHA challenge and returns a unique token that identifies it.
        /// </summary>
        /// <remarks>The generated token should be provided to the client and used in subsequent requests
        /// to verify the CAPTCHA response. The CAPTCHA challenge is stored with an expiration time as configured in the
        /// options. If the token is not used before expiration, the challenge will no longer be valid.</remarks>
        /// <returns>A string containing the unique token for the newly created CAPTCHA challenge. This token can be used to
        /// retrieve or validate the CAPTCHA.</returns>
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

        /// <summary>
        /// Retrieves the CAPTCHA image associated with the specified token.
        /// </summary>
        /// <param name="token">The unique token identifying the CAPTCHA to retrieve. Cannot be null or empty.</param>
        /// <returns>A byte array containing the CAPTCHA image data.</returns>
        /// <exception cref="CaptchaNotFoundException">Thrown if no CAPTCHA is found for the specified token.</exception>
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

        /// <summary>
        /// Validates a CAPTCHA response using the provided validation request data.
        /// </summary>
        /// <param name="data">The CAPTCHA validation request containing the user response and related information. Cannot be null.</param>
        /// <returns>A result indicating whether the CAPTCHA validation succeeded, including any relevant error information.</returns>
        public CaptchaValidationResult ValidateCaptcha(CaptchaValidateRequest data)
        {
            return _validateCaptchaService.ValidateCaptcha(data);
        }
    }
}
