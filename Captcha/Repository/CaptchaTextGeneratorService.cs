namespace Captcha.Repository
{
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
