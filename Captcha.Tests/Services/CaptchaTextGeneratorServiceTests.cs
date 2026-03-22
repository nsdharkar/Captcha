using Captcha.Configuration;
using Captcha.Interfaces;
using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace Captcha.Tests.Services
{
    public class CaptchaTextGeneratorServiceTests
    {
        [Fact]
        public void GenerateCaptchaText_ShouldReturnCorrectLength()
        {
            var random = new Mock<IRandomProvider>();
            random.Setup(x => x.Next(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(1);

            var logger = new Mock<ILogger<CaptchaTextGeneratorService>>();

            var options = new CaptchaOptions();

            var service = new CaptchaTextGeneratorService(
                random.Object,
                logger.Object,
                options);

            var result = service.GenerateCaptchaText();

            result.Length.Should().Be(6);
        }

        [Fact]
        public void GenerateCaptchaText_ShouldContainOnlyValidCharacters()
        {
            var random = new Mock<IRandomProvider>();
            random.Setup(x => x.Next(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(1);

            var logger = new Mock<ILogger<CaptchaTextGeneratorService>>();

            var options = new CaptchaOptions();

            var service = new CaptchaTextGeneratorService(
                random.Object,
                logger.Object,
                options);

            var result = service.GenerateCaptchaText();

            result.Should().MatchRegex("^[A-Za-z0-9]+$");
        }

        [Fact]
        public void GenerateCaptchaText_ShouldThrow_WhenLengthInvalid()
        {
            var random = new Mock<IRandomProvider>();
            var logger = new Mock<ILogger<CaptchaTextGeneratorService>>();

            var options = new CaptchaOptions
            {
                CaptchaLength = -1
            };
            
            var service = new CaptchaTextGeneratorService(
                random.Object,
                logger.Object,
                options);

            Assert.Throws<ArgumentOutOfRangeException>(() => service.GenerateCaptchaText());

        }
    }
}
