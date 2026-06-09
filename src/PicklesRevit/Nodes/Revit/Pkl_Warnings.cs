using Autodesk.DesignScript.Runtime;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Warnings.
    /// </summary>
    public class Pkl_Warning
    {
        internal Pkl_Warning() { }

        /// <summary>
        /// Gets the text related to a DB.FailureMessage.
        /// </summary>
        /// <param name="warning">The DB.FailureMessage.</param>
        /// <returns name="description">The warning DescriptionText.</returns>
        /// <search>warning, message, failure</search>
        public static string? GetDescription(DB.FailureMessage warning)
        {
            if (warning is null) { return null; }
            return warning.GetDescriptionText();
        }

        /// <summary>
        /// Gets the Elements related to a DB.FailureMessage.
        /// </summary>
        /// <param name="warning">The DB.FailureMessage.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="elements">The related Elements.</returns>
        /// <search>warning, message, failure, elements</search>
        public static IEnumerable<DynElement> GetFailingElements(DB.FailureMessage warning, [DefaultArgument("null")] object? docOrLinkInstance = null)
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
            return warning.GetFailingElements()
                .Select(i => i.Ext_GetDynamoElement(docHelper.Document, true));
        }
    }
}