namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Warnings.
    /// </summary>
    public class Pkl_Warning
    {
        internal Pkl_Warning() { }

        /// <summary>
        /// Gets the text related to a warning.
        /// </summary>
        /// <param name="warning">The Warning.</param>
        /// <returns name="description">The warning DescriptionText.</returns>
        /// <search>Revit.Warning.GetDescription</search>
        [NodeCategory("Query")]
        public static string? GetDescription(DynWarning warning)
        {
            if (warning is null) { return null; }
            return warning.Ext_ToFailureMessage().GetDescriptionText();
        }

        /// <summary>
        /// Gets the Elements related to a warning.
        /// </summary>
        /// <param name="warning">The Warning</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="elements">The related Elements.</returns>
        /// <search>Revit.Warning.GetFailingElements</search>
        [NodeCategory("Action")]
        public static IEnumerable<DynElement> GetFailingElements(DynWarning warning, [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            // Return the failing elements
            return warning.Ext_ToFailureMessage()
                .GetFailingElements()
                .Select(i => i.Ext_GetDynamoElement(docHelper.Document, true));
        }
    }
}