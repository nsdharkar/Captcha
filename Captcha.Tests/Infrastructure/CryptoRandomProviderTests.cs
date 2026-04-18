using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace Captcha.Tests.Infrastructure
{
    public class CryptoRandomProviderTests
    {
        /// <summary>
        /// Verifies that the Next method returns a value within the specified range.
        /// </summary>
        /// <remarks>This test ensures that the value returned by CryptoRandomProvider.Next is greater
        /// than or equal to the minimum value and less than the maximum value, as expected by the method's
        /// contract.</remarks>
        [Fact]
        public void Next_ShouldReturnValueWithinRange()
        {
            var logger = new Mock<ILogger<CryptoRandomProvider>>();
            var random = new CryptoRandomProvider(logger.Object);

            var result = random.Next(1, 10);

            result.Should().BeInRange(1, 9);
        }

        /// <summary>
        /// Verifies that the Next method throws an ArgumentException when the specified minimum value is greater than
        /// the maximum value.
        /// </summary>
        /// <remarks>This test ensures that the Next method enforces correct argument validation by
        /// throwing an exception if the minimum value exceeds the maximum value.</remarks>
        [Fact]
        public void Next_ShouldThrow_WhenMinGreaterThanMax()
        {
            var logger = new Mock<ILogger<CryptoRandomProvider>>();
            var random = new CryptoRandomProvider(logger.Object);

            Assert.Throws<ArgumentException>(() =>
                random.Next(10, 5));
        }
    }
}
