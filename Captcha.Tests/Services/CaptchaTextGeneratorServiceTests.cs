using Captcha.Configuration;
using Captcha.Interfaces;
using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace Captcha.Tests.Services
{
    /// <summary>
    /// Contains unit tests for the CaptchaTextGeneratorService class, verifying correct CAPTCHA text generation
    /// behavior and input validation.
    /// </summary>
    /// <remarks>These tests ensure that the generated CAPTCHA text meets expected length and character
    /// constraints, and that invalid configuration options result in appropriate exceptions. The tests use mock
    /// dependencies to isolate the service's logic.</remarks>
    public class CaptchaTextGeneratorServiceTests
    {
        /// <summary>
        /// Verifies that the GenerateCaptchaText method returns a string with the expected length.
        /// </summary>
        /// <remarks>This test ensures that the generated CAPTCHA text meets the default length
        /// requirement, helping to validate the correctness of the CaptchaTextGeneratorService
        /// implementation.</remarks>
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

        /// <summary>
        /// Verifies that the GenerateCaptchaText method returns a string containing only valid alphanumeric characters.
        /// </summary>
        /// <remarks>This test ensures that the generated CAPTCHA text does not include any characters
        /// outside the ranges A-Z, a-z, or 0-9.</remarks>
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

        /// <summary>
        /// Verifies that the GenerateCaptchaText method throws an ArgumentOutOfRangeException when the CaptchaLength
        /// option is set to an invalid value.
        /// </summary>
        /// <remarks>This test ensures that the CaptchaTextGeneratorService enforces input validation by
        /// throwing the appropriate exception when an invalid captcha length is provided.</remarks>
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
