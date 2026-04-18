using Captcha.Configuration;
using Captcha.Interfaces;
using Captcha.Models;

namespace Captcha.Repository
{
    /// <summary>
    /// Provides functionality to validate CAPTCHA responses using a backing store and hash verification service.
    /// </summary>
    /// <remarks>This service enforces CAPTCHA expiration and one-time use policies. It is typically used to
    /// prevent automated submissions and ensure that user input matches the generated CAPTCHA challenge. Thread safety
    /// and error handling are managed internally. For most scenarios, use this service through the
    /// IValidateCaptchaService interface.</remarks>
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

        /// <summary>
        /// Validates a CAPTCHA response using the provided request data.
        /// </summary>
        /// <remarks>Removes the CAPTCHA from the store after validation to enforce one-time use. If the
        /// CAPTCHA has expired or the token is invalid, the method returns a failure result.</remarks>
        /// <param name="data">The CAPTCHA validation request containing the user input and token. Cannot be null.</param>
        /// <returns>A CaptchaValidationResult indicating whether the CAPTCHA was successfully validated. Returns a failure
        /// result if the request is invalid, the token is expired or invalid, or the user input does not match the
        /// expected value.</returns>
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
