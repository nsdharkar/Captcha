using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace Captcha.Tests.Infrastructure
{
    public class CryptoRandomProviderTests
    {
        [Fact]
        public void Next_ShouldReturnValueWithinRange()
        {
            var logger = new Mock<ILogger<CryptoRandomProvider>>();
            var random = new CryptoRandomProvider(logger.Object);

            var result = random.Next(1, 10);

            result.Should().BeInRange(1, 9);
        }

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
