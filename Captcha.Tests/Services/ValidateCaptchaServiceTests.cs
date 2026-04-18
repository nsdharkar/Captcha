using Captcha.Configuration;
using Captcha.Interfaces;
using Captcha.Models;
using Captcha.Repository;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Captcha.Tests.Services
{
    /// <summary>
    /// Contains unit tests for the ValidateCaptchaService class, verifying correct CAPTCHA validation behavior.
    /// </summary>
    /// <remarks>These tests ensure that the service correctly handles various validation scenarios, including
    /// null requests, expired CAPTCHAs, and successful validations. The tests use mock dependencies to isolate
    /// the service's logic.</remarks>
    public class ValidateCaptchaServiceTests
    {
        private readonly Mock<ICaptchaStore> _store = new();
        private readonly Mock<ICaptchaHashService> _hash = new();
        private readonly Mock<ILogger<ValidateCaptchaService>> _logger = new();
        private static readonly object _lock = new();

        /// <summary>
        /// Creates and configures a new instance of the ValidateCaptchaService for use in tests.
        /// </summary>
        /// <returns>A new instance of ValidateCaptchaService initialized with test dependencies and default options.</returns>
        private ValidateCaptchaService CreateService()
        {
            var options = new CaptchaOptions
            {
                ExpirationMinutes = 5
            };

            return new ValidateCaptchaService(
                _logger.Object,
                _store.Object,
                options,
                _hash.Object);
        }

        /// <summary>
        /// Verifies that the ValidateCaptcha method returns a failure result when the request parameter is null.
        /// </summary>
        /// <remarks>This test ensures that passing a null request to ValidateCaptcha is handled as an
        /// invalid input, resulting in a failure outcome.</remarks>
        [Fact]
        public void ValidateCaptcha_ShouldReturnFailure_WhenRequestNull()
        {
            var service = CreateService();

            var result = service.ValidateCaptcha(null!);

            result.IsSuccess.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that the ValidateCaptcha method returns a failure result when the specified token is not found in
        /// the store.
        /// </summary>
        /// <remarks>This test ensures that the service correctly handles the scenario where a nonexistent
        /// token is provided, resulting in an unsuccessful validation.</remarks>
        [Fact]
        public void ValidateCaptcha_ShouldReturnFailure_WhenTokenNotFound()
        {
            _store.Setup(x => x.Get(It.IsAny<string>()))
                  .Returns((CaptchaData)null!);

            var service = CreateService();

            var result = service.ValidateCaptcha(new CaptchaValidateRequest
            {
                Token = "token",
                UserInput = "ABC"
            });

            result.IsSuccess.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that the ValidateCaptcha method returns a failure result when the captcha has expired.
        /// </summary>
        /// <remarks>This unit test ensures that expired captchas are correctly identified and rejected by
        /// the validation logic.</remarks>
        [Fact]
        public void ValidateCaptcha_ShouldReturnFailure_WhenExpired()
        {
            _store.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new CaptchaData
                {
                    CaptchaHashValue = "hash",
                    CaptchaCreatedAt = DateTime.UtcNow.AddMinutes(-10)
                });

            var service = CreateService();

            var result = service.ValidateCaptcha(new CaptchaValidateRequest
            {
                Token = "token",
                UserInput = "ABC"
            });

            result.IsSuccess.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that the ValidateCaptcha method returns a successful result when provided with valid captcha input.
        /// </summary>
        /// <remarks>This unit test ensures that the captcha validation logic correctly identifies valid
        /// input and produces a success outcome. It uses mocked dependencies to simulate a valid captcha
        /// scenario.</remarks>
        [Fact]
        public void ValidateCaptcha_ShouldReturnSuccess_WhenValid()
        {
            _store.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new CaptchaData
                {
                    CaptchaHashValue = "hash",
                    CaptchaCreatedAt = DateTime.UtcNow
                });

            _hash.Setup(x => x.Verify(It.IsAny<string>(), "hash"))
                 .Returns(true);

            var service = CreateService();

            var result = service.ValidateCaptcha(new CaptchaValidateRequest
            {
                Token = "token",
                UserInput = "ABC"
            });

            result.IsSuccess.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that only one successful CAPTCHA validation is allowed when multiple concurrent validation attempts
        /// are made.
        /// </summary>
        /// <remarks>This test ensures thread safety and correct behavior of the CAPTCHA validation logic
        /// under concurrent access. It confirms that only a single validation attempt can succeed, preventing multiple
        /// uses of the same CAPTCHA.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Fact]
        public async Task ValidateCaptcha_ShouldAllowOnlyOneSuccessfulValidation()
        {
            var store = new Mock<ICaptchaStore>();
            var hash = new Mock<ICaptchaHashService>();
            var logger = new Mock<ILogger<ValidateCaptchaService>>();

            var captchaData = new CaptchaData
            {
                CaptchaHashValue = "hash",
                CaptchaCreatedAt = DateTime.UtcNow
            };

            store.Setup(x => x.Get(It.IsAny<string>()))
                 .Returns(() =>
                 {
                     lock (_lock)
                     {
                         var temp = captchaData;
                         captchaData = null; // simulate removal
                         return temp;
                     }
                 });

            hash.Setup(x => x.Verify(It.IsAny<string>(), "hash"))
                .Returns(true);

            var service = new ValidateCaptchaService(
                logger.Object,
                store.Object,
                new CaptchaOptions { ExpirationMinutes = 5 },
                hash.Object);

            var request = new CaptchaValidateRequest
            {
                Token = "token",
                UserInput = "ABC123"
            };

            var tasks = Enumerable.Range(0, 20)
                .Select(_ => Task.Run(() => service.ValidateCaptcha(request)));

            var results = await Task.WhenAll(tasks);

            results.Count(r => r.IsSuccess).Should().Be(1);
        }

        /// <summary>
        /// Verifies that the ValidateCaptcha method fails when the same CAPTCHA token is reused for validation.
        /// </summary>
        /// <remarks>This test ensures that a CAPTCHA token cannot be used more than once for successful
        /// validation. The first validation attempt should succeed, while subsequent attempts with the same token
        /// should fail, enforcing single-use token behavior.</remarks>
        [Fact]
        public void ValidateCaptcha_ShouldFail_WhenTokenReused()
        {
            var store = new Mock<ICaptchaStore>();
            var hash = new Mock<ICaptchaHashService>();
            var logger = new Mock<ILogger<ValidateCaptchaService>>();

            var captcha = new CaptchaData
            {
                CaptchaHashValue = "hash",
                CaptchaCreatedAt = DateTime.UtcNow
            };

            store.SetupSequence(x => x.Get(It.IsAny<string>()))
                .Returns(captcha)
                .Returns((CaptchaData)null!);

            hash.Setup(x => x.Verify(It.IsAny<string>(), "hash"))
                .Returns(true);

            var service = new ValidateCaptchaService(
                logger.Object,
                store.Object,
                new CaptchaOptions(),
                hash.Object);

            var request = new CaptchaValidateRequest
            {
                Token = "token",
                UserInput = "ABC123"
            };

            var first = service.ValidateCaptcha(request);
            var second = service.ValidateCaptcha(request);

            first.IsSuccess.Should().BeTrue();
            second.IsSuccess.Should().BeFalse();
        }
    }
}
