using Captcha.Repository;
using FluentAssertions;


namespace Captcha.Tests.Security
{
    public class CaptchaHashServiceTests
    {
        private readonly CaptchaHashService _service = new();

        /// <summary>
        /// Verifies that the Hash method returns a non-empty, non-whitespace string when provided with a valid input.
        /// </summary>
        /// <remarks>This test ensures that the Hash method produces a meaningful hash value for a typical
        /// input string. It does not validate the format or cryptographic strength of the hash.</remarks>
        [Fact]
        public void Hash_ShouldReturnNonEmptyHash()
        {
            var result = _service.Hash("ABC123");

            result.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Verifies that the Verify method returns true when the provided value matches the given hash.
        /// </summary>
        /// <remarks>This test ensures that the hashing service correctly validates a value against its
        /// hash, confirming expected behavior for matching inputs.</remarks>
        [Fact]
        public void Verify_ShouldReturnTrue_WhenHashMatches()
        {
            var value = "ABC123";
            var hash = _service.Hash(value);

            var result = _service.Verify(value, hash);

            result.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that the Verify method returns false when the provided input does not match the expected hash.
        /// </summary>
        /// <remarks>This test ensures that the hash verification logic correctly identifies non-matching
        /// values and does not produce false positives.</remarks>
        [Fact]
        public void Verify_ShouldReturnFalse_WhenHashDoesNotMatch()
        {
            var hash = _service.Hash("ABC123");

            var result = _service.Verify("WRONG", hash);

            result.Should().BeFalse();
        }
    }
}
