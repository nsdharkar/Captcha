using SixLabors.Fonts;

namespace Captcha.Interfaces
{
    public interface ICaptchaFontLoader
    {
        IReadOnlyList<FontFamily> LoadFonts();
    }
}
