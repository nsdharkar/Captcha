namespace Captcha.Repository
{
    /// <summary>
    /// Provides a set of valid alphanumeric characters for use in validation or filtering operations.
    /// </summary>
    /// <remarks>The character set includes uppercase and lowercase English letters and digits from 1 to 9 and
    /// 0. This class can be used as a reference for allowed characters in scenarios such as input validation, random
    /// string generation, or encoding.</remarks>
    public class ValidChars
    {
        public static readonly char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();
    }
}
