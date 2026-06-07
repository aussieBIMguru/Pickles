internal static class Ext_Pickles
{
    internal static string Ext_ToMessage(this PKL_WARNING warning)
    {
        return warning switch
        {
            PKL_WARNING.UNKNOWN => "Unknown warning",
            PKL_WARNING.NO_DOC_OR_LINK => "Document/RevitLinkInstance input was not valid.",
            PKL_WARNING.INVALID_INPUTS => "Inputs are invalid, please review them.",
            PKL_WARNING.KEY_VALUE_MISMATCH => "Key and value lists are not equal in length.",
            _ => warning.ToString()
        };
    }
}