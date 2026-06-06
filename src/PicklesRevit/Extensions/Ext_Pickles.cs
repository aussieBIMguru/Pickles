internal static class Ext_Pickles
{
    internal static string Ext_ToMessage(this PKL_WARNING warning)
    {
        return warning switch
        {
            PKL_WARNING.UNKNOWN => "Unknown warning",
            PKL_WARNING.NO_DOC_OR_LINK => "Document/RevitLinkInstance input was not valid.",
            _ => warning.ToString()
        };
    }
}