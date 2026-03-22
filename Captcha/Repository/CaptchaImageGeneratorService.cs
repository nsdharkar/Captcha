using Captcha.Configuration;
using Captcha.ExceptionHandling;
using Captcha.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Captcha.Repository
{
    /// <summary>
    /// Provides functionality for generating CAPTCHA images with randomized fonts, rotations, and noise to enhance
    /// security against automated recognition.
    /// </summary>
    /// <remarks>This service is typically used in web applications or other scenarios requiring human
    /// verification through CAPTCHA validation. The generated images are PNG-encoded and incorporate various
    /// obfuscation techniques to make automated text recognition more difficult. Thread safety depends on the
    /// underlying dependencies; it is recommended to use a separate instance per request or ensure thread-safe usage of
    /// dependencies.</remarks>
    public class CaptchaImageGeneratorService : ICaptchaImageGeneratorService
    {
        private readonly ICaptchaFontProvider _captchaFontProvider;
        private readonly IRandomProvider _randomProvider;
        private readonly ILogger<CaptchaImageGeneratorService> _logger;
        private readonly CaptchaOptions _options;

        public CaptchaImageGeneratorService(ICaptchaFontProvider captchaFontProvider,
            IRandomProvider randomProvider,
            ILogger<CaptchaImageGeneratorService> logger,
            CaptchaOptions options)
        {
            _captchaFontProvider = captchaFontProvider;
            _randomProvider = randomProvider;
            _logger = logger;
            this._options = options;
        }

        /// <summary>
        /// Generates a CAPTCHA image based on the specified text and returns the image as a PNG-encoded byte array.
        /// </summary>
        /// <remarks>The generated image includes background noise and distortion to enhance security
        /// against automated recognition. The output format is always PNG. This method is not thread-safe.</remarks>
        /// <param name="captchaText">The text to be rendered in the CAPTCHA image. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <returns>A byte array containing the PNG-encoded CAPTCHA image representing the provided text.</returns>
        /// <exception cref="CaptchaGenerationException">Thrown when the CAPTCHA text is null, empty, or if an error occurs during image generation.</exception>
        public byte[] GenerateCaptchaImage(string captchaText)
        {
            if (string.IsNullOrWhiteSpace(captchaText))
            {
                _logger.LogWarning("Captcha generation failed: captcha text is null or empty.");
                throw new CaptchaGenerationException("Captcha text cannot be null or empty.");
            }

            try
            {
                _logger.LogDebug("Starting Captcha Image Generation.");

                using var image = new Image<Rgba32>(_options.Width, _options.Height);

                DrawBackground(image);
                DrawCaptchaText(image, captchaText);
                DrawNoiseDots(image);
                DrawNoiseLines(image);
                ApplyDistortion(image);

                using var ms = new MemoryStream();
                image.Save(ms, new PngEncoder());

                _logger.LogDebug("Captcha image generated successfully.");

                return ms.ToArray();
            }
            catch (ExternalException ex)
            {
                _logger.LogError(ex,
                "GDI+ error while generating CAPTCHA image for text length {CaptchaLength}", captchaText.Length);
                throw new CaptchaGenerationException("Captcha image generation failed.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error while generating CAPTCHA image");
                throw new CaptchaGenerationException("Unexpected error during captcha image generation.", ex);
            }
        }

        /// <summary>
        /// Renders the specified CAPTCHA text onto the provided image using randomized font, color, and rotation for
        /// each character.
        /// </summary>
        /// <remarks>This method applies randomization to the font, color, and rotation of each character
        /// to increase CAPTCHA complexity and resist automated recognition.</remarks>
        /// <param name="image">The image on which the CAPTCHA text will be drawn. Must not be null.</param>
        /// <param name="captchaText">The text to render as the CAPTCHA challenge. Each character is drawn individually with random visual
        /// variations.</param>
        private void DrawCaptchaText(Image<Rgba32> image, string captchaText)
        {
            var textcolor = Color.FromRgb(
                    (byte)_randomProvider.Next(_options.MinTextColor, _options.MaxTextColor),
                    (byte)_randomProvider.Next(_options.MinTextColor, _options.MaxTextColor),
                    (byte)_randomProvider.Next(_options.MinTextColor, _options.MaxTextColor));

            var charSpacing = 35;
            var baselineY = 20;

            image.Mutate(ctx =>
            {
                for (int i = 0; i < captchaText.Length; i++)
                {
                    var font = _captchaFontProvider.GetRandomFont(_options.FontSize);
                    float angle = _randomProvider.Next(_options.RotationMin, _options.RotationMax);
                    var position = new PointF(i * charSpacing, baselineY);

                    var options = new DrawingOptions
                    {
                        Transform = Matrix3x2.CreateRotation(
                        MathF.PI * angle / 180f,
                        position)
                    };

                    ctx.DrawText(
                        options,
                        captchaText[i].ToString(),
                        font,
                        textcolor,
                        position);
                }
            });
        }

        /// <summary>
        /// Fills the specified image with a randomly generated background color within the configured color range.
        /// </summary>
        /// <remarks>The background color is generated using random RGB values within the minimum and
        /// maximum color bounds specified in the options. This method modifies the provided image in place.</remarks>
        /// <param name="image">The image to fill with the background color. Must not be null.</param>
        private void DrawBackground(Image<Rgba32> image)
        {
            var color = Color.FromRgb(
                (byte)_randomProvider.Next(_options.MinBGColor, _options.MaxBGColor),
                (byte)_randomProvider.Next(_options.MinBGColor, _options.MaxBGColor),
                (byte)_randomProvider.Next(_options.MinBGColor, _options.MaxBGColor));

            image.Mutate(ctx => ctx.Fill(color));
        }

        /// <summary>
        /// Adds a specified number of random light gray noise dots to the provided image to increase visual complexity.
        /// </summary>
        /// <remarks>This method is typically used to make automated recognition of the image more
        /// difficult, such as in CAPTCHA generation. The number and placement of dots are determined by configuration
        /// options and a random number provider.</remarks>
        /// <param name="image">The image to which noise dots will be added. Must not be null.</param>
        private void DrawNoiseDots(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                for (int i = 0; i < _options.NoiseDots; i++)
                {
                    ctx.Fill(
                        Color.LightGray,
                        new EllipsePolygon(
                            _randomProvider.Next(0, _options.Width),
                            _randomProvider.Next(0, _options.Height),
                            1.5f
                            )
                        );
                }
            });

        }

        /// <summary>
        /// Draws a series of random noise lines onto the specified image to add visual distortion.
        /// </summary>
        /// <remarks>The number of noise lines and their positions are determined by the current options
        /// and a random number provider. This method is typically used to increase the difficulty of automated image
        /// recognition, such as in CAPTCHA generation.</remarks>
        /// <param name="image">The image on which the noise lines are drawn. Must not be null.</param>
        private void DrawNoiseLines(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                for (int i = 0; i < _options.NoiseLines; i++)
                {
                    ctx.DrawLine(
                        Color.Gray,
                        2,
                        new PointF[]
                        {
                            new (_randomProvider.Next(0, _options.Width), _randomProvider.Next(0, _options.Height)),
                            new (_randomProvider.Next(0, _options.Width), _randomProvider.Next(0, _options.Height))
                        });
                }
            });
        }

        /// <summary>
        /// Applyes a Gaussian blur distortion effect to the provided image to further obfuscate the visual content
        /// </summary>
        /// <param name="image"></param>
        private void ApplyDistortion(Image<Rgba32> image)
        {
            image.Mutate(ctx =>
            {
                ctx.GaussianBlur(0.5f);
            });
        }
    }
}
