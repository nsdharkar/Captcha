using Captcha.Configuration;
using Captcha.Interfaces;
using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Captcha.Models;

namespace Captcha.Tests.Services
{
    public class CaptchaServiceTests
    {
        /// <summary>
        /// Verifies that the CreateCaptcha method stores the generated CAPTCHA in the underlying store.
        /// </summary>
        /// <remarks>This test ensures that when CreateCaptcha is called, the ICaptchaStore.Store method
        /// is invoked exactly once, confirming that the CAPTCHA data is persisted as expected.</remarks>
        [Fact]
        public void CreateCaptcha_ShouldStoreCaptcha()
        {
            var text = new Mock<ICaptchaTextGeneratorService>();
            var image = new Mock<ICaptchaImageGeneratorService>();
            var store = new Mock<ICaptchaStore>();
            var validate = new Mock<IValidateCaptchaService>();
            var hash = new Mock<ICaptchaHashService>();
            var logger = new Mock<ILogger<CaptchaService>>();
            var options = new CaptchaOptions();

            text.Setup(x => x.GenerateCaptchaText())
                .Returns("ABC123");

            hash.Setup(x => x.Hash(It.IsAny<string>()))
                .Returns("hash");

            var service = new CaptchaService(
                text.Object,
                image.Object,
                store.Object,
                validate.Object,
                logger.Object,
                options,
                hash.Object);

            var token = service.CreateCaptcha();

            token.Should().NotBeNullOrWhiteSpace();

            store.Verify(x => x.Store(
                It.IsAny<string>(),
                It.IsAny<CaptchaData>(),
                It.IsAny<TimeSpan>()),
                Times.Once);
        }

        /// <summary>
        /// Verifies that the CreateCaptcha method generates unique tokens when invoked concurrently.
        /// </summary>
        /// <remarks>This test ensures that concurrent calls to CreateCaptcha do not produce duplicate
        /// tokens, validating thread safety and uniqueness guarantees under load.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task CreateCaptcha_ShouldGenerateUniqueTokens_UnderConcurrency()
        {
            var text = new Mock<ICaptchaTextGeneratorService>();
            var image = new Mock<ICaptchaImageGeneratorService>();
            var store = new Mock<ICaptchaStore>();
            var validate = new Mock<IValidateCaptchaService>();
            var hash = new Mock<ICaptchaHashService>();
            var logger = new Mock<ILogger<CaptchaService>>();

            text.Setup(x => x.GenerateCaptchaText()).Returns("ABC123");
            hash.Setup(x => x.Hash(It.IsAny<string>())).Returns("hash");

            var service = new CaptchaService(
                text.Object,
                image.Object,
                store.Object,
                validate.Object,
                logger.Object,
                new CaptchaOptions(),
                hash.Object);

            var tasks = Enumerable.Range(0, 100)
                .Select(_ => Task.Run(() => service.CreateCaptcha()));

            var tokens = await Task.WhenAll(tasks);

            tokens.Should().OnlyHaveUniqueItems();
        }

        /// <summary>
        /// Verifies that the CaptchaService can generate captchas concurrently under high load without errors.
        /// </summary>
        /// <remarks>This test simulates 1,000 parallel captcha generation requests to ensure the service
        /// handles concurrent operations reliably. It is intended to validate thread safety and performance under
        /// stress conditions.</remarks>
        /// <returns></returns>
        [Fact]
        public async Task CaptchaService_ShouldHandleHighLoadGeneration()
        {
            var text = new Mock<ICaptchaTextGeneratorService>();
            var image = new Mock<ICaptchaImageGeneratorService>();
            var store = new Mock<ICaptchaStore>();
            var validate = new Mock<IValidateCaptchaService>();
            var hash = new Mock<ICaptchaHashService>();
            var logger = new Mock<ILogger<CaptchaService>>();

            text.Setup(x => x.GenerateCaptchaText()).Returns("ABC123");
            hash.Setup(x => x.Hash(It.IsAny<string>())).Returns("hash");

            var service = new CaptchaService(
                text.Object,
                image.Object,
                store.Object,
                validate.Object,
                logger.Object,
                new CaptchaOptions(),
                hash.Object);

            var tasks = Enumerable.Range(0, 1000)
                .Select(_ => Task.Run(() => service.CreateCaptcha()));

            var tokens = await Task.WhenAll(tasks);

            tokens.Length.Should().Be(1000);
        }

        /// <summary>
        /// Verifies that the CaptchaImageGeneratorService can handle concurrent image generation requests without
        /// errors.
        /// </summary>
        /// <remarks>This test ensures that the service is thread-safe and produces the expected number of
        /// CAPTCHA images when accessed concurrently. It simulates multiple parallel requests to validate the service's
        /// reliability under load.</remarks>
        /// <returns></returns>
        [Fact]
        public async Task CaptchaImageGenerator_ShouldHandleLoad()
        {
            var fontProvider = new Mock<ICaptchaFontProvider>();
            var random = new Mock<IRandomProvider>();
            var logger = new Mock<ILogger<CaptchaImageGeneratorService>>();
            var options = new CaptchaOptions();

            fontProvider.Setup(x => x.GetRandomFont(It.IsAny<float>()))
                .Returns(SixLabors.Fonts.SystemFonts.CreateFont("Arial", 20));

            random.Setup(x => x.Next(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(5);

            var service = new CaptchaImageGeneratorService(
                fontProvider.Object,
                random.Object,
                logger.Object,
                options);

            var tasks = Enumerable.Range(0, 200)
                .Select(_ => Task.Run(() =>
                    service.GenerateCaptchaImage("ABC123")));

            var images = await Task.WhenAll(tasks);

            images.Should().HaveCount(200);
        }
    }
}
