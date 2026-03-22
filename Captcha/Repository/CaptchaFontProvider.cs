using Captcha.ExceptionHandling;
using Captcha.Interfaces;
using SixLabors.Fonts;

namespace Captcha.Repository
{
    public class CaptchaFontProvider : ICaptchaFontProvider
    {
        private readonly List<FontFamily> _fontFamilies;
        private readonly IRandomProvider _randomProvider;
        private readonly ILogger<CaptchaFontProvider> _logger;

        public CaptchaFontProvider(ICaptchaFontLoader loader, IRandomProvider randomProvider, ILogger<CaptchaFontProvider> logger)
        {
            _fontFamilies = loader.LoadFonts().ToList();
            _randomProvider = randomProvider;
            _logger = logger;
        }

        /// <summary>
        /// Selects a random font from the available font families and creates a bold font of the specified size.
        /// </summary>
        /// <param name="size">The size, in points, of the font to create. Must be greater than zero.</param>
        /// <returns>A bold Font instance from a randomly selected font family, with the specified size.</returns>
        /// <exception cref="CaptchaGenerationException">Thrown if the font families collection is not initialized or contains no fonts.</exception>
        public Font GetRandomFont(float size)
        {
            if (_fontFamilies is null || _fontFamilies.Count == 0)
            {
                throw new CaptchaGenerationException(
                    "Font families not initialized properly.");
            }

            var family = _fontFamilies[_randomProvider.Next(0,_fontFamilies.Count)];

            _logger?.LogDebug(
            "Selected CAPTCHA font: {FontName}, Size: {FontSize}",
            family.Name,
            size
            );

            return family.CreateFont(size, FontStyle.Bold);

        }
    }
}
