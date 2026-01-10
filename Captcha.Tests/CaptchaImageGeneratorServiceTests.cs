using Captcha.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Captcha.Tests
{
    public class CaptchaImageGeneratorServiceTests
    {
        private readonly Mock<ILogger<CaptchaImageGeneratorService>> _loggerMock;
        private readonly CaptchaImageGeneratorService _service;

        public CaptchaImageGeneratorServiceTests()
        {
            _loggerMock = new Mock<ILogger<CaptchaImageGeneratorService>>();
            _service = new CaptchaImageGeneratorService(_loggerMock.Object);
        }

        [Fact]
        public void GenerateCaptchaImage_ValidText_ReturnsImageBytes()
        {
            // Arrange
            var captchaText = "AB12CD";

            // Act
            var result = _service.GenerateCaptchaImage(captchaText);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.Length > 100); // PNG header + data

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public void GenerateCaptchaImage_NullText_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _service.GenerateCaptchaImage(null!)
            );

            Assert.Equal("captchaText", ex.ParamName);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );
        }

        [Fact]
        public void GenerateCaptchaImage_EmptyText_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                _service.GenerateCaptchaImage(string.Empty)
            );

            Assert.Equal("captchaText", ex.ParamName);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once
            );
        }
    }
}
