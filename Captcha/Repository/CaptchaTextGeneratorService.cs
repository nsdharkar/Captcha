using Captcha.Configuration;
using Captcha.ExceptionHandling;
using Captcha.Interfaces;

namespace Captcha.Repository
{
    /// <summary>
    /// Provides functionality for generating random CAPTCHA text strings of a specified length.
    /// </summary>
    /// <remarks>This service is typically used to create CAPTCHA challenges for user authentication or
    /// verification scenarios. The generated text consists of randomly selected characters from a predefined set.
    /// Logging is performed for both successful and failed generation attempts. This class is thread-safe for
    /// concurrent use.</remarks>
    public class CaptchaTextGeneratorService : ICaptchaTextGeneratorService
    {
        private readonly IRandomProvider _randomProvider;
        private readonly ILogger<CaptchaTextGeneratorService> _logger;
        private readonly CaptchaOptions _options;

        public CaptchaTextGeneratorService(IRandomProvider randomProvider, ILogger<CaptchaTextGeneratorService> logger,
            CaptchaOptions options)
        {
            _randomProvider = randomProvider;
            _logger = logger;
            this._options = options;
        }

        public string GenerateCaptchaText()
        {
            if (_options.CaptchaLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(_options.CaptchaLength));

            _logger?.LogDebug("Captcha text generated successfully. Length: {Length}", _options.CaptchaLength);

            try
            {
                return new string(
                Enumerable.Range(0, _options.CaptchaLength)
                          .Select(x => ValidChars.Chars[_randomProvider.Next(0, ValidChars.Chars.Length)])
                          .ToArray());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Captcha generation failed");

                throw new CaptchaGenerationException("Failed to generate captcha text", ex);
            }
        }
    }
}
