using SixLabors.Fonts;

namespace Captcha.Repository
{
    public class CaptchaFontManager
    {
        private static readonly List<FontFamily> _fontFamilies;
        private static readonly Random _random = new();

        static CaptchaFontManager()
        {
            var fontCollection = new FontCollection();
            _fontFamilies = new List<FontFamily>();

            var fontsPath = Path.Combine(AppContext.BaseDirectory, "Fonts");

            foreach (var fontFile in Directory.GetFiles(fontsPath, "*.ttf"))
            {
                var family = fontCollection.Add(fontFile);
                _fontFamilies.Add(family);
            }

            if (_fontFamilies.Count == 0)
                throw new InvalidOperationException("No fonts found in Fonts directory.");
        }

        public static Font GetRandomFont(float size)
        {
            var family = _fontFamilies[_random.Next(_fontFamilies.Count)];
            return family.CreateFont(size, FontStyle.Bold);
        }
    }
}
