using Captcha.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Captcha.Models;

namespace Captcha.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CaptchaController : ControllerBase
    {
        private readonly ICaptchaTextGeneratorService _textGeneratorService;

        private readonly ICaptchaImageGeneratorService _imageGeneratorService;

        private readonly ICaptchaStore _captchaStore;

        private readonly ILogger<CaptchaController> _logger;

        public CaptchaController(
            ICaptchaTextGeneratorService textGeneratorService,
            ICaptchaImageGeneratorService imageGeneratorService,
            ICaptchaStore captchaStore,
            ILogger<CaptchaController> logger)
        {
            _textGeneratorService = textGeneratorService;
            _imageGeneratorService = imageGeneratorService;
            _captchaStore = captchaStore;
            _logger = logger;
        }

        [HttpGet("CreateCaptcha")]
        public IActionResult GenerateText()
        {
            try
            {
                var token = Guid.NewGuid().ToString("N");

                var _captchaData = new CaptchaData
                {
                    CaptchaValue = _textGeneratorService.GenerateCaptchaText(),
                    CaptchaCreatedAt = DateTime.UtcNow
                };

                _captchaStore.Store(token, _captchaData, TimeSpan.FromMinutes(5));

                Response.Headers["Captcha-Token"] = token;

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error generating captcha.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetCaptcha")]
        public IActionResult GetCaptchaImage(string token)
        {
            try
            {
                var data = _captchaStore.Get(token);
                if (data == null)
                {
                    return BadRequest("Captcha expired or Invalid token");
                }

                byte[] imageBytes = _imageGeneratorService.GenerateCaptchaImage(data.CaptchaValue!);
                return File(imageBytes, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error generating captcha image.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("validate")]
        public IActionResult ValidateCaptcha([FromBody] CaptchaValidateRequest request)
        {
            
            try
            {
                var data = _captchaStore.Get(request.Token);

                if (data == null)
                {
                    return BadRequest("Captcha expired or Invalid token");
                }

                if (DateTime.UtcNow - data.CaptchaCreatedAt > TimeSpan.FromMinutes(5))
                {
                    _captchaStore.Remove(request.Token);
                    return BadRequest("Captcha expired.");
                }

                if (!string.Equals(
                    request.UserInput,
                    data.CaptchaValue,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Invalid captcha.");
                }

                _captchaStore.Remove(request.Token); // one-time use

                return Ok("Captcha validated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating captcha. SessionId: {SessionId}", HttpContext.Session.Id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
