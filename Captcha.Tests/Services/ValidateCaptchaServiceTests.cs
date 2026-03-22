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
    public class ValidateCaptchaServiceTests
    {
        private readonly Mock<ICaptchaStore> _store = new();
        private readonly Mock<ICaptchaHashService> _hash = new();
        private readonly Mock<ILogger<ValidateCaptchaService>> _logger = new();
        private static readonly object _lock = new();

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

        [Fact]
        public void ValidateCaptcha_ShouldReturnFailure_WhenRequestNull()
        {
            var service = CreateService();

            var result = service.ValidateCaptcha(null!);

            result.IsSuccess.Should().BeFalse();
        }

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
