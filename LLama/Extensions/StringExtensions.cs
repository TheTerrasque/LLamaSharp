namespace LLama.Extensions
{
    public static class StringExtension
    {
        public static string Capitalize(this string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }
    }
}