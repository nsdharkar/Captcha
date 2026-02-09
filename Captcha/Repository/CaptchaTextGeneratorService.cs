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
        private readonly ILogger<CaptchaTextGeneratorService> _logger;
        public CaptchaTextGeneratorService(ILogger<CaptchaTextGeneratorService> logger)
        {
            _logger = logger;
        }

        public string GenerateCaptchaText(int capLength = 6)
        {
            try
            {
                if (capLength <= 0)
                    throw new ArgumentOutOfRangeException(
                        nameof(capLength),
                        "Captcha length must be greater than zero."
                    );

                var random = new Random();

                _logger?.LogInformation(
                "Captcha text generated successfully. Length: {Length}",
                capLength);

                return new string(
                    Enumerable.Range(0, capLength)
                              .Select(x => ValidChars._chars[random.Next(ValidChars._chars.Length)])
                              .ToArray());
            }
                        catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error generating captcha text. Length: {Length}",
                    capLength);
                throw new InvalidOperationException(
            "Error occurred while generating captcha text.",
            ex);
            }
        }
    }
}
