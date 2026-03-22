using Captcha.Repository;
using FluentAssertions;


namespace Captcha.Tests.Security
{
    public class CaptchaHashServiceTests
    {
        private readonly CaptchaHashService _service = new();

        [Fact]
        public void Hash_ShouldReturnNonEmptyHash()
        {
            var result = _service.Hash("ABC123");

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Verify_ShouldReturnTrue_WhenHashMatches()
        {
            var value = "ABC123";
            var hash = _service.Hash(value);

            var result = _service.Verify(value, hash);

            result.Should().BeTrue();
        }

        [Fact]
        public void Verify_ShouldReturnFalse_WhenHashDoesNotMatch()
        {
            var hash = _service.Hash("ABC123");

            var result = _service.Verify("WRONG", hash);

            result.Should().BeFalse();
        }
    }
}
