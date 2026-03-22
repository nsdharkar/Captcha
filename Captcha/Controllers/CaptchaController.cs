using Microsoft.AspNetCore.Mvc;
using Captcha.Models;
using Captcha.Interfaces;
using Captcha.ExceptionHandling;

namespace Captcha.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class CaptchaController : ControllerBase
    {
        private readonly ILogger<CaptchaController> _logger;
        private readonly ICaptchaService _captchaService;
        public CaptchaController(ILogger<CaptchaController> logger, ICaptchaService captchaService)
        {
            _logger = logger;
            _captchaService = captchaService;
        }

        /// <summary>
        /// Generates a new CAPTCHA token for client-side validation.
        /// </summary>
        /// <remarks>Clients should use the returned token to request a CAPTCHA image or to validate user
        /// input in subsequent requests. This endpoint does not return the CAPTCHA image itself, only the token
        /// required for further CAPTCHA operations.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a JSON object with the generated CAPTCHA token.</returns>
        [HttpGet("CreateCaptcha")]
        public IActionResult GenerateCaptchaToken()
        {
            var token = _captchaService.CreateCaptcha();
            return Ok(new { Token = token });
        }

        /// <summary>
        /// Returns a PNG image representing a CAPTCHA challenge associated with the specified token.
        /// </summary>
        /// <param name="token">The unique token identifying the CAPTCHA challenge. Cannot be null, empty, or whitespace.</param>
        /// <returns>An <see cref="IActionResult"/> containing the CAPTCHA image as a PNG file.</returns>
        /// <exception cref="TokenRequiredException">Thrown if <paramref name="token"/> is null, empty, or consists only of whitespace.</exception>
        [HttpGet("GetCaptcha")]
        public IActionResult GetCaptchaImage(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new TokenRequiredException("Token required.");
    
            byte[] imageBytes = _captchaService.GetCaptchaImage(token);
            return File(imageBytes, "image/png");
        }

        /// <summary>
        /// Validates a CAPTCHA response submitted by the client.
        /// </summary>
        /// <remarks>Use this endpoint to verify that a user has successfully completed a CAPTCHA
        /// challenge before allowing access to protected resources.</remarks>
        /// <param name="request">The CAPTCHA validation request containing the user's response and related data. Cannot be null.</param>
        /// <returns>An HTTP 200 OK result if the CAPTCHA is valid; otherwise, an HTTP 400 Bad Request result with an error
        /// message.</returns>
        [HttpPost("Validate")]
        public IActionResult ValidateCaptcha([FromBody] CaptchaValidateRequest request)
        {
            _logger.LogInformation("Received request for validation.");

            if (request is null)
                return BadRequest("Invalid request");

            var returnMessage = _captchaService.ValidateCaptcha(request);

            if (returnMessage.IsSuccess)
                return Ok();

            return BadRequest(returnMessage.ErrorMessage);
        }
    }
}
