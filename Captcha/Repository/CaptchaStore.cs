
using Captcha.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Captcha.Repository
{
    public class CaptchaStore : ICaptchaStore
    {
        private readonly IMemoryCache _cache;

        public CaptchaStore(IMemoryCache cache)
        {
            _cache = cache;
        }
        public CaptchaData? Get(string token)
        {
            _cache.TryGetValue(token, out CaptchaData? data);
            return data;
        }

        public void Remove(string token)
        {
            _cache.Remove(token);
        }

        public void Store(string token, CaptchaData data, TimeSpan ttl)
        {
            _cache.Set(token, data, ttl);
        }
    }
}
