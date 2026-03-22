using Captcha.ExceptionHandling;
using Captcha.Interfaces;
using SixLabors.Fonts;

namespace Captcha.Repository
{
    public class CaptchaFontLoader : ICaptchaFontLoader
    {
        private List<FontFamily>? _fontFamilies;
        private ILogger? _logger;

        public CaptchaFontLoader(ILogger<CaptchaFontLoader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Loads all TrueType font families from the application's Fonts directory.
        /// </summary>
        /// <remarks>Each font file with a .ttf extension in the Fonts directory is loaded as a separate
        /// font family. Logging is performed for each successfully loaded font if a logger is available.</remarks>
        /// <returns>A read-only list of loaded font families. The list contains one entry for each valid .ttf file found in the
        /// Fonts directory.</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown if the Fonts directory does not exist in the application's base directory.</exception>
        /// <exception cref="CaptchaGenerationException">Thrown if no valid .ttf font files are found in the Fonts directory.</exception>
        public IReadOnlyList<FontFamily> LoadFonts()
        {
            var fontCollection = new FontCollection();
            _fontFamilies = new List<FontFamily>();

            var fontsPath = Path.Combine(AppContext.BaseDirectory, "Fonts");

            if (!Directory.Exists(fontsPath))
            {
                throw new DirectoryNotFoundException(
                    $"Fonts directory not found at path: {fontsPath}");
            }

            foreach (var fontFile in Directory.GetFiles(fontsPath, "*.ttf"))
            {
                var family = fontCollection.Add(fontFile);
                _fontFamilies.Add(family);

                _logger?.LogInformation(
                "Loaded CAPTCHA font: {FontName}",
                family.Name
                );
            }

            if (_fontFamilies.Count == 0)
            {
                throw new CaptchaGenerationException(
                    "No valid .ttf font files found in Fonts directory.");
            }

            return _fontFamilies;
        }
    }
}
