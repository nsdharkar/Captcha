using Captcha.Models;
using Captcha.Repository;
using Microsoft.Extensions.Caching.Memory;
using FluentAssertions;

namespace Captcha.Tests.Infrastructure
{
    public class CaptchaStoreTests
    {
        [Fact]
        public void StoreAndGet_ShouldReturnCaptcha()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var store = new CaptchaStore(cache);

            var token = "token";

            var data = new CaptchaData
            {
                CaptchaHashValue = "hash",
                CaptchaCreatedAt = DateTime.UtcNow
            };

            store.Store(token, data, TimeSpan.FromMinutes(5));

            var result = store.Get(token);

            result.Should().NotBeNull();
        }

        [Fact]
        public void Remove_ShouldDeleteCaptcha()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var store = new CaptchaStore(cache);

            var token = "token";

            store.Store(token,
                new CaptchaData { CaptchaHashValue = "hash" },
                TimeSpan.FromMinutes(5));

            store.Remove(token);

            var result = store.Get(token);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CaptchaStore_ShouldHandleConcurrentAccess()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var store = new CaptchaStore(cache);

            var tasks = Enumerable.Range(0, 50).Select(i =>
                Task.Run(() =>
                {
                    var token = $"token-{i}";

                    var data = new CaptchaData
                    {
                        CaptchaHashValue = "hash",
                        CaptchaCreatedAt = DateTime.UtcNow
                    };

                    store.Store(token, data, TimeSpan.FromMinutes(5));

                    var result = store.Get(token);

                    Assert.NotNull(result);
                }));

            await Task.WhenAll(tasks);
        }
    }
}
