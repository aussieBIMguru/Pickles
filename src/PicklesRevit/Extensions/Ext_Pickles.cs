using System.Text.RegularExpressions;

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