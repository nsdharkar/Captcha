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
        private readonly ILogger<CaptchaController> _logger;

        public CaptchaController(
            ICaptchaTextGeneratorService textGeneratorService,
            ICaptchaImageGeneratorService imageGeneratorService,
            ILogger<CaptchaController> logger)
        {
            _textGeneratorService = textGeneratorService;
            _imageGeneratorService = imageGeneratorService;
            _logger = logger;
        }

        [HttpGet("Captcha")]
        public IActionResult GenerateText()
        {
            var _captchaData = new CaptchaData
            {
                CaptchaValue = _textGeneratorService.GenerateCaptchaText(),
                CaptchaCreatedAt = DateTime.UtcNow
            };

            //if(HttpContext.Session == null)
            HttpContext.Session.SetString("Captcha", JsonSerializer.Serialize(_captchaData));

            return File(_imageGeneratorService.GenerateCaptchaImage(_captchaData.CaptchaValue), "image/png");
        }

        [HttpPost("validate")]
        public IActionResult ValidateCaptcha(string userInput)
        {
            _logger.LogInformation("ValidateCaptcha called. SessionId: {SessionId}", HttpContext.Session.Id);

            try
            {


                var _captcha = HttpContext.Session.GetString("Captcha");

                if (_captcha == null)
                {
                    _logger.LogWarning(
                    "Captcha expired or missing. SessionId: {SessionId}",
                    HttpContext.Session.Id);

                    return BadRequest("Captcha expired");
                }

                var _captchaText = JsonSerializer.Deserialize<CaptchaData>(_captcha);

                if (_captchaText != null)
                {
                    if (DateTime.UtcNow - _captchaText.CaptchaCreatedAt > TimeSpan.FromMinutes(2))
                    {
                        _logger.LogWarning(
                            "Captcha expired due to timeout. CreatedAt: {CreatedAt}, SessionId: {SessionId}",
                            _captchaText.CaptchaCreatedAt,
                            HttpContext.Session.Id);
                        return BadRequest("Captcha expired");
                    }
                    else
                        if (!string.Equals(userInput, _captchaText.CaptchaValue, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning(
                        "Invalid captcha attempt. InputLength: {Length}, SessionId: {SessionId}",
                        userInput?.Length ?? 0,
                        HttpContext.Session.Id);
                        return BadRequest("Invalid captcha");
                    }
                }
                else
                {
                    _logger.LogError("Failed to deserialize captcha data. SessionId: {SessionId}", HttpContext.Session.Id);
                    return BadRequest("Invalid captcha data");
                }

                HttpContext.Session.Remove("Captcha"); // One-time use
                _logger.LogInformation("Captcha validated successfully. SessionId: {SessionId}", HttpContext.Session.Id);
                return Ok("Valid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating captcha. SessionId: {SessionId}", HttpContext.Session.Id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
