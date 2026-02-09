using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Path = System.IO.Path;

namespace Captcha.Repository
{
    /// <summary>
    /// Provides functionality to generate CAPTCHA images from text for use in user verification scenarios.
    /// </summary>
    /// <remarks>This service creates CAPTCHA images by rendering the provided text with randomized fonts,
    /// rotations, and added visual noise to prevent automated recognition. The generated images are suitable for
    /// embedding in web applications or other user interfaces that require human verification. Thread safety is not
    /// guaranteed; create a separate instance per request if used in a multi-threaded environment.</remarks>
    public class CaptchaImageGeneratorService : ICaptchaImageGeneratorService
    {
        private readonly ILogger<CaptchaImageGeneratorService> _logger;
        
        public CaptchaImageGeneratorService(ILogger<CaptchaImageGeneratorService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generates a CAPTCHA image based on the specified text and returns the image as a PNG-encoded byte array.
        /// </summary>
        /// <remarks>The generated image includes random fonts, rotations, and noise to enhance security
        /// and prevent automated recognition. The output is suitable for use in web applications or other scenarios
        /// requiring CAPTCHA validation.</remarks>
        /// <param name="captchaText">The text to be rendered in the CAPTCHA image. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <returns>A byte array containing the PNG-encoded CAPTCHA image representing the specified text.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="captchaText"/> is null, empty, or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if an error occurs during image generation, such as a GDI+ failure.</exception>
        /// <exception cref="ApplicationException">Thrown if an unexpected error occurs while generating the CAPTCHA image.</exception>
        public byte[] GenerateCaptchaImage(string captchaText)
        {
            if (string.IsNullOrWhiteSpace(captchaText))
            {
                _logger.LogWarning("Captcha generation failed: captcha text is null or empty.");
                throw new ArgumentException("Captcha text cannot be null or empty.", nameof(captchaText));
            }

            try
            {
                _logger.LogInformation("Starting Captcha Image Generation.");

                using var image = new Image<Rgba32>(200, 60, Color.White);
                var random = new Random();

                image.Mutate(ctx =>
                {
                    for (int i = 0; i < captchaText.Length; i++)
                    {
                        var font = CaptchaFontManager.GetRandomFont(28);
                        float angle = random.Next(-25, 20);
                        var position = new PointF(i * 35, 20);

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
                            Color.Black,
                            position);
                    }

                    //Add noise dots
                    for (int i = 0; i < 300; i++)
                    {
                        ctx.Fill(
                            Color.LightGray,
                            new EllipsePolygon(
                                random.Next(200),
                                random.Next(60),
                                1.5f
                                )
                            );
                    }

                    //Add noise lines
                    for (int i = 0; i < 5; i++)
                    {
                        ctx.DrawLine(
                            Color.Gray,
                            2,
                            new PointF[]
                            {
                            new(random.Next(0, 200), random.Next(0, 60)),
                            new(random.Next(0, 200), random.Next(0, 60))
                            }
                        );
                    }

                });

                using var ms = new MemoryStream();
                image.Save(ms, new PngEncoder());

                _logger.LogInformation("Captcha image generated successfully.");

                return ms.ToArray();
            }
            catch (ExternalException ex)
            {
                _logger.LogError(ex,
                "GDI+ error while generating CAPTCHA image for text length {CaptchaLength}", captchaText.Length);
                throw new InvalidOperationException("Failed to generate captcha image.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unexpected error while generating CAPTCHA image");
                throw new ApplicationException("An unexpected error occurred while generating captcha image.", ex);
            }
        }
    }
}
