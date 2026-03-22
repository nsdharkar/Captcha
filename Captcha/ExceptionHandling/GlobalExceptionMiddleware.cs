namespace Captcha.ExceptionHandling
{
    /// <summary>
    /// Middleware that provides centralized exception handling for HTTP requests in the application pipeline.
    /// </summary>
    /// <remarks>This middleware intercepts unhandled exceptions thrown during request processing, logs the
    /// error, and returns a standardized JSON error response with an appropriate HTTP status code. It should be
    /// registered early in the middleware pipeline to ensure that exceptions from subsequent components are handled
    /// consistently.</remarks>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the GlobalExceptionMiddleware class with the specified request delegate and
        /// logger.
        /// </summary>
        /// <remarks>This middleware should be registered early in the pipeline to ensure that unhandled
        /// exceptions are properly logged and handled for all subsequent middleware components.</remarks>
        /// <param name="next">The next middleware component in the HTTP request pipeline. Cannot be null.</param>
        /// <param name="logger">The logger used to record exception details. Cannot be null.</param>
        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes an HTTP request asynchronously and handles any unhandled exceptions by logging the error and generating an
        /// appropriate response.
        /// </summary>
        /// <remarks>This method should be used as part of the ASP.NET Core middleware pipeline. If an unhandled exception
        /// occurs during request processing, the exception is logged and a standardized error response is sent to the
        /// client.</remarks>
        /// <param name="context">The HTTP context for the current request. Provides access to request and response information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception");

                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles exceptions by setting the appropriate HTTP status code and returning a JSON error response.
        /// </summary>
        /// <remarks>The method maps specific exception types to HTTP status codes: 400 for token or
        /// captcha-related errors and argument exceptions, 404 for not found errors, and 500 for all other exceptions.
        /// The response body contains a JSON object with an 'error' property describing the exception.</remarks>
        /// <param name="context">The HTTP context for the current request. Used to set the response status code and content.</param>
        /// <param name="exception">The exception to handle. Determines the HTTP status code and error message returned to the client.</param>
        /// <returns>A task that represents the asynchronous operation of writing the JSON error response to the HTTP response
        /// stream.</returns>
        private static Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            int statusCode;
            switch (exception)
            {
                case TokenRequiredException:
                    statusCode = 400;
                    break;

                case CaptchaExpiredException:
                    statusCode = 400;
                    break;

                case CaptchaInvalidException:
                    statusCode = 400;
                    break;

                case CaptchaNotFoundException:
                    statusCode = 404;
                    break;

                case ArgumentException:
                    statusCode = 400;
                    break;

                default:
                    statusCode = 500;
                    break;
            }

            var response = new
            {
                error = exception.Message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
