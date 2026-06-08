using System;
using System.Collections.Generic;
using System.Text;

namespace Pickles.Extensions
{
    internal static class Ext_String
    {
        /// <summary>
        /// Returns if a string has characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A boolean.</returns>
        public static bool Ext_HasChars(this string str)
        {
            return str?.Length > 0;
        }

        /// <summary>
        /// Returns if a string has no characters.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A boolean.</returns>
        public static bool Ext_HasNoChars(this string str)
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
        public static string Ext_DeNull(this string str, string ifNull = "", bool replaceEmpty = false)
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
        public static bool Ext_MatchAsWords(this string searchString, string matchPhrase, char splitChar = ' ', MATCH_MODE mode = MATCH_MODE.SUBSTRING_INSENSITIVE)
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
    }
}
