namespace Captcha.Configuration
{
 /// <summary>
 /// Represents configuration options for generating CAPTCHA images, including dimensions, appearance, and expiration
 /// settings.
 /// </summary>
 /// <remarks>Use this class to customize the visual and functional parameters of CAPTCHA generation, such as
 /// image size, font size, noise level, rotation, color ranges, and the length and expiration of the CAPTCHA text.
 /// Adjusting these options can help balance usability and security based on application requirements.</remarks>
    public class CaptchaOptions
    {
        public int Width { get; set; } = 200;
        public int Height { get; set; } = 60;
        public int FontSize { get; set; } = 28;

        public int RotationMin { get; set; } = -25;
        public int RotationMax { get; set; } = 20;

        public int NoiseDots { get; set; } = 300;
        public int NoiseLines { get; set; } = 5;

        public int ExpirationMinutes { get; set; } = 2;

        public int MinTextColor { get; set; } = 0;
        public int MaxTextColor { get; set; } = 120;

        public int CaptchaLength { get; set; } = 6;

        public int MinBGColor { get; set; } = 200;
        public int MaxBGColor { get; set; } = 255;
    }
}
