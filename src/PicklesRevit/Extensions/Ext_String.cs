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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="fallbackValue"></param>
        /// <returns></returns>
        internal static T Ext_EnumByName<T>(this string name, T fallbackValue) where T : struct, Enum
        {
            if (Enum.TryParse(name, out T t))
            {
                return t;
            }
            else
            {
                return fallbackValue;
            }
        }

        /// <summary>
        /// Calculates the Levenshtein Distance between this string and another string.
        /// </summary>
        public static int? Ext_LevenshteinDistance(this string source, string target)
        {
            // Null check
            if (source == null || target == null) { return null; }

            int sourceLength = source.Length;
            int targetLength = target.Length;

            // Case: Empty strings
            if (sourceLength == 0) return targetLength;
            if (targetLength == 0) return sourceLength;

            // Optimization: ensure target is shorter or equal to save memory
            if (sourceLength < targetLength)
            {
                return target.Ext_LevenshteinDistance(source);
            }

            // Maintain only the current and previous rows of the matrix
            int[] previousRow = new int[targetLength + 1];
            int[] currentRow = new int[targetLength + 1];

            // Initialize the first row
            for (int j = 0; j <= targetLength; j++)
            {
                previousRow[j] = j;
            }

            // Calculate distances
            for (int i = 0; i < sourceLength; i++)
            {
                currentRow[0] = i + 1;

                for (int j = 0; j < targetLength; j++)
                {
                    // If characters match, the cost is 0; otherwise, it's 1
                    int cost = (source[i] == target[j]) ? 0 : 1;

                    // Find the minimum of: deletion, insertion, substitution
                    currentRow[j + 1] = Math.Min(
                        Math.Min(currentRow[j] + 1, previousRow[j + 1] + 1),
                        previousRow[j] + cost
                    );
                }

                // Move to the next row (copy current to previous)
                Array.Copy(currentRow, previousRow, currentRow.Length);
            }

            return previousRow[targetLength];
        }
    }
}
