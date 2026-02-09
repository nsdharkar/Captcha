
using Captcha.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Captcha.Repository
{
    /// <summary>
    /// Provides an implementation of an in-memory store for CAPTCHA data using an IMemoryCache instance.
    /// </summary>
    /// <remarks>This class enables storage, retrieval, and removal of CAPTCHA data associated with unique
    /// tokens. It is intended for scenarios where CAPTCHA validation state needs to be maintained temporarily, such as
    /// during user verification workflows. The underlying memory cache is suitable for single-instance or
    /// non-distributed environments; for distributed scenarios, consider using a distributed cache
    /// implementation.</remarks>
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
