using Captcha.Models;

namespace Captcha.Interfaces
{
    public interface ICaptchaStore
    {
        void Store(string token, CaptchaData data, TimeSpan ttl);
        CaptchaData? Get(string token);
        void Remove(string token);
    }
}
