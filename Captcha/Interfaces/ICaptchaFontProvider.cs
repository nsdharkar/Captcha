using SixLabors.Fonts;

namespace Captcha.Interfaces
{
    public interface ICaptchaFontProvider
    {
        Font GetRandomFont(float size);
    }
}
