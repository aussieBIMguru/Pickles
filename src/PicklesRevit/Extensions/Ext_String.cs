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
    }
}
