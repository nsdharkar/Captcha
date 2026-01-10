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
    public class CaptchaFontManagerTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public CaptchaFontManagerTests()
        {
            _loggerMock = new Mock<ILogger>();

            // IMPORTANT: initialize logger BEFORE any method call
            CaptchaFontManager.Initialize(_loggerMock.Object);
        }

        [Fact]
        public void GetRandomFont_ValidSize_ReturnsFont()
        {
            // Arrange
            float fontSize = 28;

            // Act
            var font = CaptchaFontManager.GetRandomFont(fontSize);

            // Assert
            Assert.NotNull(font);
            Assert.Equal(fontSize, font.Size);
        }

        [Fact]
        public void GetRandomFont_LogsInformation()
        {
            // Act
            CaptchaFontManager.GetRandomFont(24);

            // Assert
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
        public void CaptchaFontManager_LoadsFonts_FromDisk()
        {
            var font = CaptchaFontManager.GetRandomFont(20);
            Assert.NotNull(font);
        }
    }
}
