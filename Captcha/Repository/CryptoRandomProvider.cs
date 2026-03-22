using Captcha.Interfaces;
using System.Security.Cryptography;

namespace Captcha.Repository
{
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
