namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Families.
    /// </summary>
    public class Pkl_Family
    {
        internal Pkl_Family() { }

        /// <summary>
        /// Edits a family, opening it as a FamilyDocument.
        /// </summary>
        /// <param name="family">The Family to edit.</param>
        /// <returns name="familyDocument">The Document.</returns>
        /// <search>Revit.Family.EditAsDocument</search>
        [NodeCategory("Action")]
        public static DynDocument? EditAsDocument(DynElement family)
        {
            if (family.InternalElement is DB.Family dbFamily)
            {
                DB.Document familyDoc = dbFamily.Document.EditFamily(dbFamily);
                return familyDoc.Ext_ToDynDocument();
            }
            else
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }
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

        /// <summary>
        /// Returns if the Family is modelled in place.
        /// </summary>
        /// <param name="family">The Family to query.</param>
        /// <returns name="isInPlace">If the Family is modelled in place.</returns>
        /// <search>Revit.Family.IsInPlace</search>
        [NodeCategory("Query")]
        public static bool IsInPlace(DynElement family)
        {
            if (family.InternalElement is DB.Family dbFamily)
            {
                return dbFamily.IsInPlace;
            }
            return false;
        }
    }
}