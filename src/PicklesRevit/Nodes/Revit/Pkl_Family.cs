namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Families.
    /// </summary>
    public class Pkl_Family
    {
        internal Pkl_Family() { }

        /// <summary>
        /// Edits families from a Document as Family Documents.
        /// </summary>
        /// <param name="families">The Families to edit.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="documents">The Documents.</returns>
        /// <search>Revit.Family.EditAsDocuments</search>
        [NodeCategory("Action")]
        public static List<DynDocument> EditAsDocuments(List<DynElement> families,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynDocument>();
            }

            int notFamilyCount = 0;
            List<DynDocument?> documents = new();
            DB.Document doc = docHelper.Document;

            foreach (DynElement dynElement in families)
            {
                if (dynElement?.InternalElement is DB.Family family)
                {
                    DB.Document familyDoc = doc.EditFamily(family);
                    documents.Add(familyDoc.Ext_ToDynDocument());
                }
                else
                {
                    documents.Add(null);
                    notFamilyCount++;
                }
            }

            if (notFamilyCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Some families could not be edited.");
            }

            return documents;
        }

        /// <summary>
        /// Returns the Category of a Family.
        /// </summary>
        /// <param name="family">The Family to query.</param>
        /// <returns name="category">The Category.</returns>
        /// <search>Revit.Family.Category</search>
        [NodeCategory("Query")]
        public static DynCategory? Category(DynElement family)
        {
            if (family.InternalElement is DB.Family dbFamily)
            {
                return dbFamily.FamilyCategory.Ext_ToDynCategory();
            }
            return null;
        }
    }
}