using Captcha.Configuration;
using Captcha.Interfaces;
using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Captcha.ExceptionHandling;

namespace Captcha.Tests.Image
{
    /// <summary>
    /// Contains unit tests for the CaptchaImageGeneratorService class, verifying its behavior when generating CAPTCHA
    /// images and handling invalid input.
    /// </summary>
    /// <remarks>These tests ensure that CaptchaImageGeneratorService returns valid image data for non-empty
    /// input and throws the expected exception when provided with empty text. The tests use mock dependencies to
    /// isolate the service's functionality.</remarks>
    public class CaptchaImageGeneratorServiceTests
    {
        /// <summary>
        /// Verifies that the GenerateCaptchaImage method returns a non-null, non-empty byte array representing the
        /// generated CAPTCHA image.
        /// </summary>
        /// <remarks>This test ensures that the CAPTCHA image generation functionality produces valid
        /// image data when provided with a sample input string.</remarks>
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

        /// <summary>
        /// Verifies that the GenerateCaptchaImage method throws a CaptchaGenerationException when called with an empty
        /// text value.
        /// </summary>
        /// <remarks>This test ensures that the service enforces input validation by rejecting empty
        /// CAPTCHA text. It is important for callers to provide a non-empty string when generating a CAPTCHA
        /// image.</remarks>
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
