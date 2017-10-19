namespace JeremyTCD.DotNet.Analyzers
{
    public static class MiscHelper
    {
        public static string FirstCharUpper(this string s)
        {
            char[] sChars = s.ToCharArray();
            sChars[0] = char.ToUpper(sChars[0]);

            return new string(sChars);
        }
    }
}
