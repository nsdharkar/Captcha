using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captcha.Tests
{
    public class CaptchaTextGeneratorServiceTests
    {
        private readonly Mock<ILogger<CaptchaTextGeneratorService>> _loggerMock;
        private readonly CaptchaTextGeneratorService _service;

        public CaptchaTextGeneratorServiceTests()
        {
            _loggerMock = new Mock<ILogger<CaptchaTextGeneratorService>>();
            _service = new CaptchaTextGeneratorService(_loggerMock.Object);
        }

        [Fact]
        public void GenerateCaptchaText_DefaultLength_ReturnsValidText()
        {
            // Act
            var result = _service.GenerateCaptchaText();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6, result.Length);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );
        }

        [Fact]
        public void GenerateCaptchaText_CustomLength_ReturnsCorrectLength()
        {
            // Arrange
            var length = 8;

            // Act
            var result = _service.GenerateCaptchaText(length);

            // Assert
            Assert.Equal(length, result.Length);
        }

        [Fact]
        public void GenerateCaptchaText_ZeroLength_ThrowsInvalidOperationException()
        {
            // Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _service.GenerateCaptchaText(0)
            );

            // Assert
            Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
            Assert.Contains("greater than zero", ex.InnerException!.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );
        }

        [Fact]
        public void GenerateCaptchaText_NegativeLength_ThrowsInvalidOperationException()
        {
            // Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _service.GenerateCaptchaText(-5)
            );

            // Assert
            Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );
        }
    }
}
