using SixLabors.Fonts;

namespace Captcha.Repository
{
    /// <summary>
    /// Provides functionality for loading and managing font families used in CAPTCHA generation, including selecting
    /// random fonts for rendering CAPTCHA text.
    /// </summary>
    /// <remarks>The CaptchaFontManager loads all TrueType font files (*.ttf) from a 'Fonts' directory located
    /// in the application's base directory during static initialization. If no valid fonts are found, or if the
    /// directory is missing, initialization will fail and an exception will be thrown. Logging can be enabled by
    /// calling Initialize with an ILogger instance before using font-related methods. This class is thread-safe for
    /// static usage.</remarks>
    public class CaptchaFontManager
    {
        private static readonly List<FontFamily> _fontFamilies;
        private static readonly Random _random = new();
        private static ILogger? _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
            _logger.LogInformation("CaptchaFontManager initialized.");
        }

        /// <summary>
        /// Initializes the static resources for the CaptchaFontManager class by loading all TrueType font files from
        /// the application's Fonts directory.
        /// </summary>
        /// <remarks>This static constructor ensures that all required CAPTCHA fonts are loaded and
        /// available before any static members of CaptchaFontManager are accessed. If initialization fails, the class
        /// will be unusable until the application is restarted and the issue is resolved. Logging is performed for both
        /// successful and failed font loads.</remarks>
        /// <exception cref="DirectoryNotFoundException">Thrown if the Fonts directory does not exist in the application's base directory.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no valid .ttf font files are found in the Fonts directory.</exception>
        /// <exception cref="TypeInitializationException">Thrown if an error occurs during static initialization, such as when loading fonts fails.</exception>
        static CaptchaFontManager()
        {
            try
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
                    try
                    {
                        var family = fontCollection.Add(fontFile);
                        _fontFamilies.Add(family);

                        _logger?.LogInformation(
                        "Loaded CAPTCHA font: {FontName}",
                        family.Name
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(
                        ex,
                        "Failed to load font file {FontFile}",
                        Path.GetFileName(fontFile)
                    );
                    }
                }

                if (_fontFamilies.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No valid .ttf font files found in Fonts directory.");
                }

            }
            catch (Exception ex)
            {
                _logger?.LogCritical(
               ex,
               "CaptchaFontManager failed during static initialization");

                throw new TypeInitializationException(typeof(CaptchaFontManager).FullName, ex);
            }
        }

        /// <summary>
        /// Returns a randomly selected font from the available font families, using the specified size and bold style.
        /// </summary>
        /// <param name="size">The size, in points, to use for the returned font. Must be a positive value.</param>
        /// <returns>A Font object representing a randomly chosen font family with the specified size and bold style.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the font families are not initialized or if an error occurs while retrieving the font.</exception>
        public static Font GetRandomFont(float size)
        {
            try
            {
                if (_fontFamilies == null || _fontFamilies.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Font families not initialized properly.");
                }

                var family = _fontFamilies[_random.Next(_fontFamilies.Count)];

                _logger?.LogInformation(
                "Selected CAPTCHA font: {FontName}, Size: {FontSize}",
                family.Name,
                size
                );

                return family.CreateFont(size, FontStyle.Bold);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                ex,
                "Error retrieving random font.");
                throw new InvalidOperationException(
                "Failed to retrieve CAPTCHA font.",
                ex
            );
            }
        }
    }
}
