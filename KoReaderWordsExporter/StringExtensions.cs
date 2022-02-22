using System.Text.RegularExpressions;

namespace KoReaderWordsExporter
{
    public static class StringExtensions
    {
        public static string RemoveNewLinesCharacters(this string s)
        {
            return Regex.Replace(s, @"\t|\n|\r|(\\\n)", " ");
        }
    }
}
