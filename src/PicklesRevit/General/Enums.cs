using System.Text.RegularExpressions;

namespace Pickles.Enums
{
    internal enum WARNING_TYPE
    {
        UNKNOWN = 0,
        NO_DOC_OR_LINK = 1,
        INVALID_INPUTS = 2,
        KEY_VALUE_MISMATCH = 3,
        WRONG_CATEGORY_INPUTS = 4,
        DIRECTORY_DOES_NOT_EXIST = 5
    }

    internal enum MATCH_MODE
    {
        SUBSTRING_INSENSITIVE = 0,
        SUBSTRING_SENSITIVE = 1,
        ANY_WORD = 2,
        ALL_WORDS = 3,
    }

    internal enum REGEX
    {
        DIGITS = 0
    }

    internal enum RESOURCE_TYPE
    {
        INVALID = 0,
        FILE = 1,
        DIRECTORY = 2,
        URL = 3,
    }

    internal class EnumHelpers
    {
        internal static T EnumByName<T>(string name, T fallbackValue) where T : struct, Enum
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
    }

    internal static class Ext_Pickles
    {
        internal static string Ext_ToMessage(this WARNING_TYPE warning)
        {
            return warning switch
            {
                WARNING_TYPE.UNKNOWN => "Unknown warning",
                WARNING_TYPE.NO_DOC_OR_LINK => "Document/RevitLinkInstance is invalid.",
                WARNING_TYPE.INVALID_INPUTS => "Input(s) are invalid.",
                WARNING_TYPE.KEY_VALUE_MISMATCH => "Key and value lists are not equal in length.",
                WARNING_TYPE.WRONG_CATEGORY_INPUTS => "Input(s) are of the wrong Revit category.",
                WARNING_TYPE.DIRECTORY_DOES_NOT_EXIST => "Directory does not exist.",
                _ => warning.ToString()
            };
        }

        internal static void Ext_Raise(this WARNING_TYPE warning)
        {
            LogWarningMessageEvents.OnLogWarningMessage(warning.Ext_ToMessage());
        }

        internal static Regex? Ext_ToRegex(this REGEX regex)
        {
            switch (regex)
            {
                case REGEX.DIGITS: return new Regex("^[0-9]$", RegexOptions.Compiled);
                default: return default;
            }
        }
    }
}
