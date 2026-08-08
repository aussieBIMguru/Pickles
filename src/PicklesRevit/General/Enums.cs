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
        DOC_NOT_WORKSHARED = 6,
        DOC_NOT_FAMILY = 7,
        DOC_HIGHER_VERSION = 8,
        FAMDOC_TYPE_EXISTS = 9,
        FAMDOC_PARAM_EXISTS = 10,
        FAMDOC_TYPE_NOTEXISTS = 11,
        FAMDOC_PARAM_NOTEXISTS = 12
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

    internal static class Ext_Pickles
    {
        internal static string Ext_ToMessage(this WARNING_TYPE warning, string message = null)
        {
            string warningMessage = warning switch
            {
                WARNING_TYPE.DEFAULT => message ?? "Undefined warning type.",
                WARNING_TYPE.NO_DOC_OR_LINK => "Document/RevitLinkInstance is invalid.",
                WARNING_TYPE.INVALID_INPUTS => "Input(s) are invalid.",
                WARNING_TYPE.KEY_VALUE_MISMATCH => "Inputs are not equal in length.\n\nShortest lacing will be applied.",
                WARNING_TYPE.WRONG_CATEGORY_INPUTS => "Input(s) are of the wrong Revit category.",
                WARNING_TYPE.DIRECTORY_DOES_NOT_EXIST => "Directory does not exist.",
                WARNING_TYPE.DOC_NOT_WORKSHARED => "Document is not Workshared.",
                WARNING_TYPE.DOC_NOT_FAMILY => "Document is not a Family.",
                WARNING_TYPE.DOC_HIGHER_VERSION => "Document is saved in a higher version of Revit.",
                WARNING_TYPE.FAMDOC_TYPE_EXISTS => "FamilyType by that name already exists.",
                WARNING_TYPE.FAMDOC_PARAM_EXISTS => "FamilyParameter by that name already exists.",
                WARNING_TYPE.FAMDOC_TYPE_NOTEXISTS => "FamilyType by that name not found.",
                WARNING_TYPE.FAMDOC_PARAM_NOTEXISTS => "FamilyParameter by that name not found.",
                _ => message ?? "Undefined warning type."
            };

            return $"Pickles: {warningMessage}";
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
