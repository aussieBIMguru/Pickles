namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to ViewFamilyTypes.
    /// </summary>
    public class Pkl_ViewFamilyType
    {
        internal Pkl_ViewFamilyType() { }

        /// <summary>
        /// Gets all ViewFamilyTypes of the given ViewFamily name.
        /// </summary>
        /// <param name="viewFamilyName">The ViewFamily name to collect.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="viewFamilyTypes">The ViewFamilyTypes of the given ViewFamily</returns>
        /// <search>Revit.ViewFamilyType.GetByViewFamily</search>
        [NodeCategory("Action")]
        public static IList<DynElement> GetByViewFamily(string viewFamilyName,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            // Get ViewFamily value
            DB.ViewFamily viewFamily = viewFamilyName.Ext_EnumByName(DB.ViewFamily.Invalid);

            // Set and return the outputs
            return docHelper.Document.Ext_CollectByClass<DB.ViewFamilyType>(elementTypes: true)
                .Where(v => v.ViewFamily == viewFamily)
                .Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Gets the ViewFamilyType of provided ViewFamily and name.
        /// </summary>
        /// <param name="viewFamilyName">The ViewFamily name to target.</param>
        /// <param name="typeName">The ViewFamilyType name to get.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="viewFamilyType">The ViewFamilyType.</returns>
        /// <search>Revit.ViewFamilyType.GetByViewFamilyAndName</search>
        [NodeCategory("Action")]
        public static DynElement? GetByViewFamilyAndName(string viewFamilyName, string typeName,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return null;
            }

            // Get ViewFamily value
            DB.ViewFamily viewFamily = viewFamilyName.Ext_EnumByName(DB.ViewFamily.Invalid);

            // Set and return the outputs
            return docHelper.Document.Ext_CollectByClass<DB.ViewFamilyType>(elementTypes: true)
                .FirstOrDefault(v => v.ViewFamily == viewFamily && v.Name == typeName)?
                .Ext_ToDynElement(true);
        }

        /// <summary>
        /// Gets the Views of the given ViewFamilyType.
        /// </summary>
        /// <param name="viewFamilyType">The ViewFamilyType.</param>
        /// <returns name="views">All Views of that ViewFamilyType.</returns>
        /// <search>Revit.ViewFamilyType.GetAllViews</search>
        [NodeCategory("Action")]
        public static IList<DynElement> GetAllViews(DynElement viewFamilyType)
        {
            if (viewFamilyType.InternalElement is DB.ViewFamilyType vft)
            {
                DB.ElementId vftId = vft.Id;

                return vft.Document.Ext_CollectByClass<DB.View>()
                    .Where(v => !v.IsTemplate)
                    .Where(v => v.GetTypeId() == vftId)
                    .Ext_ToDynamoElements(true);
            }

            return new List<DynElement>();
        }

        /// <summary>
        /// Gets the default View Template that is applied to new views of a ViewFamilyType.
        /// </summary>
        /// <param name="viewFamilyType">The ViewFamilyType.</param>
        /// <returns name="views">The View Template, if any.</returns>
        /// <search>Revit.ViewFamilyType.GetDefaultViewTemplate</search>
        [NodeCategory("Action")]
        public static DynElement? GetDefaultViewTemplate(DynElement viewFamilyType)
        {
            if (viewFamilyType.InternalElement is DB.ViewFamilyType vft)
            {
                return vft.DefaultTemplateId.Ext_GetDynamoElement(vft.Document, true);
            }
            return null;
        }

        /// <summary>
        /// Gets the ViewFamily of the ViewFamilyType.
        /// </summary>
        /// <param name="viewFamilyType">The ViewFamilyType to query.</param>
        /// <returns name="viewFamilyName">The ViewFamily.</returns>
        /// <search>Revit.ViewFamilyType.ViewFamily</search>
        [NodeCategory("Query")]
        public static string? ViewFamily(DynElement viewFamilyType)
        {
            if (viewFamilyType.InternalElement is DB.ViewFamilyType vft)
            {
                return vft.ViewFamily.ToString();
            }
            return null;
        }
    }
}