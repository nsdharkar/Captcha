using Captcha.Models;
using Captcha.Repository;
using Microsoft.Extensions.Caching.Memory;
using FluentAssertions;

namespace Captcha.Tests.Infrastructure
{
    /// <summary>
    /// Contains unit tests for the CaptchaStore class, verifying correct storage, retrieval, removal, and concurrent
    /// access handling of CAPTCHA data.
    /// </summary>
    /// <remarks>These tests ensure that CaptchaStore behaves as expected when storing and retrieving CAPTCHA
    /// entries, removing entries, and handling concurrent operations. The tests use an in-memory cache to simulate
    /// storage and validate thread safety and correctness under parallel access scenarios.</remarks>
    public class CaptchaStoreTests
    {
        /// <summary>
        /// Verifies that storing and retrieving a CAPTCHA using the CaptchaStore returns the expected CAPTCHA data.
        /// </summary>
        /// <remarks>This test ensures that after storing a CaptchaData instance with a specific token,
        /// retrieving it with the same token returns a non-null result. It validates the basic functionality of the
        /// CaptchaStore's Store and Get methods.</remarks>
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

        /// <summary>
        /// Verifies that removing a CAPTCHA using the CaptchaStore deletes the expected CAPTCHA data.
        /// </summary>
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

        /// <summary>
        /// Verifies that the CaptchaStore can handle concurrent access by multiple threads without data loss or
        /// corruption.
        /// </summary>
        /// <remarks>This test simulates concurrent storage and retrieval of CAPTCHA data to ensure thread
        /// safety and correct behavior under parallel operations.</remarks>
        /// <returns></returns>
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
