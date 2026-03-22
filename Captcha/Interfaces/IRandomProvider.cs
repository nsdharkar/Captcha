namespace Captcha.Interfaces
{
    public interface IRandomProvider
    {
        int Next(int min, int max);
    }
}
