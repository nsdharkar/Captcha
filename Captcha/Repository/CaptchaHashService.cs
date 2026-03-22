using Captcha.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Captcha.Repository
{
    public class CaptchaHashService : ICaptchaHashService
    {
        /// <summary>
        /// Computes a SHA-256 hash of the specified string and returns the result as a Base64-encoded string.
        /// </summary>
        /// <remarks>The method uses UTF-8 encoding to convert the input string to bytes before hashing.
        /// The output can be used for secure storage or comparison of sensitive data, but is not suitable for password
        /// storage without additional security measures such as salting.</remarks>
        /// <param name="value">The input string to hash. Cannot be null.</param>
        /// <returns>A Base64-encoded string representing the SHA-256 hash of the input value.</returns>
        public string Hash(string value)
        {
            using var sha = SHA256.Create();

            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Determines whether the specified value matches the provided hash.
        /// </summary>
        /// <remarks>Comparison is performed using ordinal string comparison. Use this method to validate
        /// that a value corresponds to a previously generated hash.</remarks>
        /// <param name="value">The plain text value to verify against the hash.</param>
        /// <param name="hash">The hash to compare with the computed hash of the value.</param>
        /// <returns>true if the computed hash of the value matches the provided hash; otherwise, false.</returns>
        public bool Verify(string value, string hash)
        {
            var computedHash = Hash(value);
            return string.Equals(computedHash, hash, StringComparison.Ordinal);
        }
    }
}
