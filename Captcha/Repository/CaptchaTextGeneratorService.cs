namespace Captcha.Repository
{
    public class CaptchaTextGeneratorService : ICaptchaTextGeneratorService
    {
        public string GenerateCaptchaText(int capLength = 6)
        {
            var random = new Random();
            return new string(
                Enumerable.Range(0, capLength)
                          .Select(x => ValidChars._chars[random.Next(ValidChars._chars.Length)])
                          .ToArray()
                );
            //throw new NotImplementedException();
        }
    }
}
