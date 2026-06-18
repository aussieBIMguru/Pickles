using System.Globalization;

namespace Pickles.Extensions
{
    internal static class Ext_String
    {
        /// <summary>
        /// Returns if a string has characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A boolean.</returns>
        internal static bool Ext_HasChars(this string str)
        {
            return str?.Length > 0;
        }

        /// <summary>
        /// Returns if a string has no characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A boolean.</returns>
        internal static bool Ext_HasNoChars(this string str)
        {
            return !str.Ext_HasChars();
        }

        /// <summary>
        /// If string is null, substitutes it.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="ifNull">Value to replace if null (optional).</param>
        /// <param name="replaceEmpty">Catch empty string case also.</param>
        /// <returns>A string.</returns>
        internal static string Ext_DeNull(this string str, string ifNull = "", bool replaceEmpty = false)
        {
            if (replaceEmpty)
            {
                return str.Ext_HasChars() ? str : ifNull;
            }
            else
            {
                return str ?? ifNull;
            }
        }

        /// <summary>
        /// Searches a string more extensively.
        /// </summary>
        /// <param name="searchString">A string to search.</param>
        /// <param name="matchPhrase">The phrase to search for.</param>
        /// <param name="splitChar">Character to split the phrase into.</param>
        /// <param name="mode">The mode by which to match against.</param>
        /// <returns>A boolean.</returns>
        internal static bool Ext_MatchAsWords(this string searchString, string matchPhrase, char splitChar = ' ', MATCH_MODE mode = MATCH_MODE.SUBSTRING_INSENSITIVE)
        {
            // Normalise nulls
            searchString ??= string.Empty;
            matchPhrase ??= string.Empty;

            // No filter text -> everything passes
            if (matchPhrase.Ext_HasNoChars()) { return true; }

            // Whole phrase (default)
            if (mode == MATCH_MODE.SUBSTRING_INSENSITIVE)
            {
                return searchString?.IndexOf(matchPhrase, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            else if (mode == MATCH_MODE.SUBSTRING_SENSITIVE)
            {
                return searchString?.IndexOf(matchPhrase, StringComparison.Ordinal) >= 0;
            }

            // Split the parts, track matches
            var matchParts = matchPhrase.Split(splitChar);
            if (matchParts.Length == 0) { return true; }

            // For each match, check if it is contained
            int matches = 0;

            foreach (var matchPart in matchParts)
            {
                if (searchString.IndexOf(matchPart, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matches++;
                }
            }

            // If match all, must be equal, otherwise any match
            if (mode == MATCH_MODE.ALL_WORDS)
            {
                return matches == matchParts.Length;
            }
            // Otherwise 1 or more matches suffices
            else
            {
                return matches > 0;
            }
        }

        /// <summary>
        /// Convert a string to a nullable integer.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <returns>A nullable integer.</returns>
        internal static int? Ext_ToIntOrNull(this string text)
        {
            if (text.Ext_HasChars() &&
                int.TryParse(text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out int x))
            {
                return x;
            }

            return null;
        }

        /// <summary>
        /// Convert a string to an integer.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <param name="failValue">The value to return on failure to convert.</param>
        /// <returns>An integer.</returns>
        internal static int Ext_ToIntWithFallback(this string text, int failValue = 0)
        {
            return text.Ext_ToIntOrNull() ?? failValue;
        }

        /// <summary>
        /// Convert a string to a nullable double.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <returns>A nullable double.</returns>
        internal static double? Ext_ToDoubleOrNull(this string text)
        {
            if (text.Ext_HasChars() &&
                double.TryParse(text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out double x))
            {
                return x;
            }

            return null;
        }

        /// <summary>
        /// Convert a string to a double.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <param name="failValue">The value to return on failure to convert.</param>
        /// <returns>A double.</returns>
        internal static double Ext_ToDoubleOrFallback(this string text, double failValue = 0.0)
        {
            return text.Ext_ToDoubleOrNull() ?? failValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="badChars"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        internal static string Ext_ReplaceBarChars(this string text, char[] badChars, string replace)
        {
            if (text.Ext_HasNoChars())
            {
                return text;
            }
            
            var sb = new System.Text.StringBuilder();

            foreach (char c in text)
            {
                if (badChars.Contains(c))
                {
                    sb.Append(replace);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
