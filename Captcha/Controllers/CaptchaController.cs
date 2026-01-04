using Captcha.Repository;
using Microsoft.AspNetCore.Http;
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

        public CaptchaController(
            ICaptchaTextGeneratorService textGeneratorService,
            ICaptchaImageGeneratorService imageGeneratorService)
        {
            _textGeneratorService = textGeneratorService;
            _imageGeneratorService = imageGeneratorService;
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
            var _captcha = HttpContext.Session.GetString("Captcha");

            if (_captcha == null) return BadRequest("Captcha expired");

            var _captchaText = JsonSerializer.Deserialize<CaptchaData>(_captcha);

            if (_captchaText != null)
            {
                if (DateTime.UtcNow - _captchaText.CaptchaCreatedAt > TimeSpan.FromMinutes(2))
                    return BadRequest("Captcha expired");
                else
                    if (!string.Equals(userInput, _captchaText.CaptchaValue, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid captcha");
            }

            HttpContext.Session.Remove("Captcha"); // One-time use
            return Ok("Valid");
        }
    }
}
