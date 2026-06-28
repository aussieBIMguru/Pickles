using System.Text.RegularExpressions;

namespace Pickles.Enums
{
    internal enum WARNING_TYPE
    {
        DEFAULT = 0,
        NO_DOC_OR_LINK = 1,
        INVALID_INPUTS = 2,
        KEY_VALUE_MISMATCH = 3,
        WRONG_CATEGORY_INPUTS = 4,
        DIRECTORY_DOES_NOT_EXIST = 5,
        DOC_NOT_WORKSHARED = 6
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
        internal static string Ext_ToMessage(this WARNING_TYPE warning, string message = null)
        {
            return warning switch
            {
                WARNING_TYPE.DEFAULT => message ?? "Unknown warning type.",
                WARNING_TYPE.NO_DOC_OR_LINK => "Document/RevitLinkInstance is invalid.",
                WARNING_TYPE.INVALID_INPUTS => "Input(s) are invalid.",
                WARNING_TYPE.KEY_VALUE_MISMATCH => "Inputs are not equal in length.\n\nShortest lacing will be applied.",
                WARNING_TYPE.WRONG_CATEGORY_INPUTS => "Input(s) are of the wrong Revit category.",
                WARNING_TYPE.DIRECTORY_DOES_NOT_EXIST => "Directory does not exist.",
                WARNING_TYPE.DOC_NOT_WORKSHARED => "The current Document is not Workshared.",
                _ => message ?? "Unknown warning type."
            };
        }

        internal static void Ext_Raise(this WARNING_TYPE warning, string message = null)
        {
            LogWarningMessageEvents.OnLogWarningMessage(warning.Ext_ToMessage(message));
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
