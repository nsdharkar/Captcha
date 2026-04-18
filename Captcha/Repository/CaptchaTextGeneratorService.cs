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

        /// <summary>
        /// Generates a random CAPTCHA text string using the configured character set and length.
        /// </summary>
        /// <remarks>The generated CAPTCHA text consists of randomly selected characters from the allowed
        /// character set. Logging is performed for both successful and failed generation attempts if a logger is
        /// configured.</remarks>
        /// <returns>A string containing the generated CAPTCHA text. The length of the string is determined by the configured
        /// CAPTCHA length.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the configured CAPTCHA length is less than or equal to zero.</exception>
        /// <exception cref="CaptchaGenerationException">Thrown if an error occurs during CAPTCHA text generation.</exception>
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
