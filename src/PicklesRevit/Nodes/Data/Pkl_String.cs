using System.Globalization;

namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to Strings.
    /// </summary>
    public class Pkl_String
    {
        internal Pkl_String() { }

        private static readonly char[] CHARS_ILLEGAL_WINDOWS = {
            '/', '?', '<', '>', '\\', ':', '*', '|', '"', '^'};

        private static readonly char[] CHARS_ILLEGAL_REVIT = {
            '\\',':','{','}','[',']','|',';','<','>','?','`','~'};

        /// <summary>
        /// Generates an alphabet.
        /// </summary>
        /// <param name="upper">Return in upper case.</param>
        /// <returns name="alphabet">The alphabet.</returns>
        /// <search>Data.String.Alphabet</search>
        [NodeCategory("Create")]
        public static IList<string> Alphabet(bool upper = true)
        {
            char startAt = upper ? 'A' : 'a';
            
            return Enumerable.Range(startAt, 26)
                .Select(i => ((char)i).ToString())
                .ToList();
        }

        /// <summary>
        /// Converts a string to a number, with an optional fallback value in the case of failure.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <param name="failValue">The value to return if conversion fails.</param>
        /// <returns name="number">The converted number, or the fallback value if conversion fails.</returns>
        /// <search>Data.String.StringToNumber</search>
        [NodeCategory("Action")]
        public static double? ToNumber(
            string text,
            [DefaultArgument("null")] double? failValue = null)
        {
            return text.Ext_ToDoubleOrNull() ?? failValue;
        }

        /// <summary>
        /// Converts a number to a string, ensuring no trailing zeroes occur.
        /// </summary>
        /// <param name="number">The text to enumerate.</param>
        /// <returns name="text">The string.</returns>
        /// <search>Data.String.NumberToString</search>
        [NodeCategory("Action")]
        public static string FromNumber(double number)
        {
            return number.ToString("G", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts a string to a GUID.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <returns name="guid">A System.Guid (null if unsuccessful).</returns>
        /// <search>Data.String.ToGuid</search>
        [NodeCategory("Action")]
        public static System.Guid? ToGuid(string text)
        {
            return Guid.TryParse(text, out var g) ? g : null;
        }

        /// <summary>
        /// Splits a string at each uppercase character.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <returns name="parts">The split parts.</returns>
        /// <search>Data.String.SplitAtUpperCase</search>
        [NodeCategory("Action")]
        public static IList<string> SplitByUpperChars(string text)
        {
            if (text.Ext_HasNoChars())
            {
                return new List<string>();
            }

            List<string> parts = new List<string>();
            int start = 0;

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                {
                    parts.Add(text.Substring(start, i - start));
                    start = i;
                }
            }

            parts.Add(text.Substring(start));
            return parts;
        }

        /// <summary>
        /// Enumerates a string into a list of single-character strings.
        /// </summary>
        /// <param name="text">The text to enumerate.</param>
        /// <returns name="characters">The characters.</returns>
        /// <search>Data.String.ToCharacters</search>
        [NodeCategory("Action")]
        public static IList<string> ToCharacters(string text)
        {
            return text
                .Select(c => c.ToString())
                .ToList();
        }

        /// <summary>
        /// Replaces empty strings or nulls.
        /// </summary>
        /// <param name="text">The string to check.</param>
        /// <param name="replaceWith">The string to replace with.</param>
        /// <returns name="outcome">The outcome.</returns>
        /// <search>Data.String.ReplaceNullOrEmpty</search>
        [NodeCategory("Action")]
        public static string ReplaceNullOrEmpty(string text, string replaceWith)
        {
            return text.Ext_HasNoChars() ? replaceWith : text;
        }

        /// <summary>
        /// Converts a delimited string into a 2D string matrix.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="columnDelimiter">Delimiter for columns.</param>
        /// <param name="rowDelimiter">Delimiter for rows.</param>
        /// <returns name="matrix">A 2D list of strings.</returns>
        /// <search>Data.String.ToMatrix</search>
        [NodeCategory("Action")]
        public static List<List<string>> ToMatrix(string text,
            string columnDelimiter = ",", string rowDelimiter = "\n")
        {
            List<List<string>> matrix = new();
            if (text is null) { return matrix; }
            
            foreach (var row in text.Split(rowDelimiter))
            {
                matrix.Add(row.Split(columnDelimiter).ToList());
            }

            return matrix;
        }

        /// <summary>
        /// Converts a 2D string matrix into a single delimited string.
        /// </summary>
        /// <param name="matrix">The 2D string list.</param>
        /// <param name="columnDelimiter">Delimiter between columns.</param>
        /// <param name="rowDelimiter">Delimiter between rows.</param>
        /// <returns name="text">The merged string.</returns>
        /// <search>Data.String.FromMatrix</search>
        [NodeCategory("Action")]
        public static string FromMatrix(List<List<string>> matrix,
            string columnDelimiter = ",", string rowDelimiter = "\n")
        {
            return string.Join(rowDelimiter, matrix.Select(row =>
                    string.Join(columnDelimiter, row ?? new List<string>()))
            );
        }

        /// <summary>
        /// Replaces illegal Windows filename characters in a string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="replace">The replacement string for illegal characters.</param>
        /// <returns name="legalized">The resulting string.</returns>
        /// <returns name="changed">Was the string changed.</returns>
        /// <search>Data.String.MakeWindowsLegal</search>
        [MultiReturn("legalized", "changed")]
        [NodeCategory("Action")]
        public static Dictionary<string, object> MakeWindowsLegal(string text, string replace = "")
        {
            string legalString = text.Ext_ReplaceBarChars(CHARS_ILLEGAL_WINDOWS, replace);
            
            return new Dictionary<string, object>()
            {
                { "legalized", legalString },
                { "changed", text != legalString }
            };
        }

        /// <summary>
        /// Replaces illegal Revit characters in a string.
        /// </summary>
        /// <param name="text">The input string.</param>
        /// <param name="replace">The replacement string for illegal characters.</param>
        /// <returns name="legalized">The resulting string.</returns>
        /// <returns name="changed">Was the string changed.</returns>
        /// <search>Data.String.MakeRevitLegal</search>
        [MultiReturn("legalized", "changed")]
        [NodeCategory("Action")]
        public static Dictionary<string, object> MakeRevitLegal(string text, string replace = "")
        {
            string legalString = text.Ext_ReplaceBarChars(CHARS_ILLEGAL_REVIT, replace);

            return new Dictionary<string, object>()
            {
                { "legalized", legalString },
                { "changed", text != legalString }
            };
        }

        /// <summary>
        /// Runs a string through a regular expression check.
        /// </summary>
        /// <param name="text">The string to check.</param>
        /// <param name="regex">The regex pattern to check against.</param>
        /// <returns name="passes">True if the string matches the regex.</returns>
        /// <search>Data.String.Regex</search>
        [NodeCategory("Query")]
        public static bool Regex(string text, string regex)
        {
            if (text == null || regex == null)
            {
                return false;
            }

            return System.Text.RegularExpressions.Regex.IsMatch(text, regex);
        }
    }
}
