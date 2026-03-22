using Captcha.Configuration;
using Captcha.Interfaces;
using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Captcha.ExceptionHandling;

namespace Captcha.Tests.Image
{
    public class CaptchaImageGeneratorServiceTests
    {
        [Fact]
        public void GenerateCaptchaImage_ShouldReturnBytes()
        {
            var fontProvider = new Mock<ICaptchaFontProvider>();
            var random = new Mock<IRandomProvider>();
            var logger = new Mock<ILogger<CaptchaImageGeneratorService>>();
            var options = new CaptchaOptions();

            fontProvider.Setup(x => x.GetRandomFont(It.IsAny<float>()))
                .Returns(SixLabors.Fonts.SystemFonts.CreateFont("Arial", 20));

            var service = new CaptchaImageGeneratorService(
                fontProvider.Object,
                random.Object,
                logger.Object,
                options);

            var result = service.GenerateCaptchaImage("ABC123");

            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GenerateCaptchaImage_ShouldThrow_WhenTextEmpty()
        {
            var fontProvider = new Mock<ICaptchaFontProvider>();
            var random = new Mock<IRandomProvider>();
            var logger = new Mock<ILogger<CaptchaImageGeneratorService>>();

            var options = new CaptchaOptions();

            var service = new CaptchaImageGeneratorService(
                fontProvider.Object,
                random.Object,
                logger.Object,
                options);

            //Assert.Throws<Exception>(() => service.GenerateCaptchaImage(""));
            Assert.Throws<CaptchaGenerationException>(() => service.GenerateCaptchaImage(""));
        }
    }
}
