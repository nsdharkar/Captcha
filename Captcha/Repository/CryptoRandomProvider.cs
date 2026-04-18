using Captcha.Interfaces;
using System.Security.Cryptography;

namespace Captcha.Repository
{
    /// <summary>
    /// Provides cryptographically secure random number generation services using system-provided algorithms.
    /// </summary>
    /// <remarks>This class implements the IRandomProvider interface and generates random numbers suitable for
    /// security-sensitive operations. It uses the underlying platform's cryptographic random number generator to ensure
    /// high-quality randomness. Instances of this class are typically used where predictable or repeatable random
    /// sequences are not acceptable, such as in cryptographic key generation or secure tokens.</remarks>
    public class CryptoRandomProvider : IRandomProvider
    {
        private readonly ILogger<CryptoRandomProvider> _logger;

        public CryptoRandomProvider(ILogger<CryptoRandomProvider> logger)
        {
            _logger = logger;
        }

        public int Next(int min, int max)
        {
            if (min >= max)
                throw new ArgumentException("min must be less than max");

            return RandomNumberGenerator.GetInt32(min, max);
        }
    }
}
