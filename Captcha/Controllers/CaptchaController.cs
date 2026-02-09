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

        /// <summary>
        /// Generates a new CAPTCHA challenge and returns a token for client-side validation.
        /// </summary>
        /// <remarks>The generated CAPTCHA token is valid for 5 minutes. Clients should use the returned
        /// token to retrieve or validate the CAPTCHA as required by subsequent API endpoints. If an error occurs during
        /// generation, the method returns a 500 Internal Server Error.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a JSON object with a unique CAPTCHA token in the <c>Token</c>
        /// property. The response also includes the token in the <c>Captcha-Token</c> response header.</returns>
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

        /// <summary>
        /// Generates and returns a CAPTCHA image associated with the specified token.
        /// </summary>
        /// <remarks>This endpoint is typically used to display a CAPTCHA image to users as part of a
        /// verification process. The token must correspond to a valid, unexpired CAPTCHA entry.</remarks>
        /// <param name="token">The unique token identifying the CAPTCHA to retrieve. Cannot be null or empty.</param>
        /// <returns>An image file containing the CAPTCHA in PNG format if the token is valid; otherwise, a 400 Bad Request
        /// response if the token is invalid or expired, or a 500 Internal Server Error response if an unexpected error
        /// occurs.</returns>
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

        /// <summary>
        /// Validates a CAPTCHA response submitted by the client.
        /// </summary>
        /// <remarks>This endpoint enforces a one-time use policy for CAPTCHA tokens and considers tokens
        /// expired after five minutes. The comparison is case-insensitive. If validation fails, the token is
        /// invalidated and cannot be reused.</remarks>
        /// <param name="request">The CAPTCHA validation request containing the user input and the associated token. Cannot be null.</param>
        /// <returns>An <see cref="OkObjectResult"/> if the CAPTCHA is valid; otherwise, a <see cref="BadRequestObjectResult"/>
        /// with an error message if the CAPTCHA is invalid or expired, or a <see cref="StatusCodeResult"/> with status
        /// code 500 if an internal error occurs.</returns>
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
