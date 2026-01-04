using SixLabors.Fonts;

namespace Captcha.Repository
{
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
