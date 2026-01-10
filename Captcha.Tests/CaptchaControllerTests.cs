using Captcha.Controllers;
using Captcha.Models;
using Captcha.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace Captcha.Tests
{
    public class CaptchaControllerTests
    {
        private readonly Mock<ICaptchaTextGeneratorService> _textServiceMock;
        private readonly Mock<ICaptchaImageGeneratorService> _imageServiceMock;
        private readonly Mock<ILogger<CaptchaController>> _loggerMock;
        private readonly CaptchaController _controller;

        public CaptchaControllerTests()
        {
            _textServiceMock = new Mock<ICaptchaTextGeneratorService>();
            _imageServiceMock = new Mock<ICaptchaImageGeneratorService>();
            _loggerMock = new Mock<ILogger<CaptchaController>>();

            _controller = new CaptchaController(
                _textServiceMock.Object,
                _imageServiceMock.Object,
                _loggerMock.Object
            );

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public void GenerateText_ReturnsImageAndStoresCaptchaInSession()
        {
            // Arrange
            var captchaText = "ABC123";
            var imageBytes = new byte[] { 1, 2, 3 };

            _textServiceMock
                .Setup(x => x.GenerateCaptchaText(captchaText.Length))
                .Returns(captchaText);

            _imageServiceMock
                .Setup(x => x.GenerateCaptchaImage(captchaText))
                .Returns(imageBytes);

            // Act
            var result = _controller.GenerateText();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/png", fileResult.ContentType);
            Assert.Equal(imageBytes, fileResult.FileContents);

            var sessionValue = _controller.HttpContext.Session.GetString("Captcha");
            Assert.NotNull(sessionValue);

            var captchaData = JsonSerializer.Deserialize<CaptchaData>(sessionValue!);
            Assert.Equal(captchaText, captchaData!.CaptchaValue);

            _textServiceMock.Verify(x => x.GenerateCaptchaText(captchaText.Length), Times.Once);
            _imageServiceMock.Verify(x => x.GenerateCaptchaImage(captchaText), Times.Once);
        }

        [Fact]
        public void GenerateText_WhenExceptionOccurs_Returns500()
        {
            // Arrange
            var captchaText = "ABC123";
            _textServiceMock
                .Setup(x => x.GenerateCaptchaText(captchaText.Length))
                .Throws(new Exception("Failure"));

            // Act
            var result = _controller.GenerateText();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("Internal server error", statusResult.Value);

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
