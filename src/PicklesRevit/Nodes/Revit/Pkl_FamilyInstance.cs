namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to FamilyInstances.
    /// </summary>
    public class Pkl_FamilyInstance
    {
        internal Pkl_FamilyInstance() { }

        /// <summary>
        /// Gets the parent of the FamilyInstance is nested.
        /// </summary>
        /// <param name="familyInstance">The FamilyInstance.</param>
        /// <returns name="superComponent">The parent of the FamilyInstance.</returns>
        /// <search>Revit.FamilyInstance.SuperComponent</search>
        [NodeCategory("Query")]
        public static DynElement? GetSuperComponent(DynFamilyInstance familyInstance)
        {
            if (familyInstance.InternalElement is DB.FamilyInstance revitFamilyInstance)
            {
                return revitFamilyInstance.SuperComponent.Ext_ToDynElement(true);
            }
            return null;
        }

        /// <summary>
        /// Returns if the FamilyInstance is hand flipped.
        /// </summary>
        /// <param name="familyInstance">The FamilyInstance.</param>
        /// <returns name="handFlipped">If the FamilyInstance is hand flipped.</returns>
        /// <search>Revit.FamilyInstance.IsHandFlipped</search>
        [NodeCategory("Query")]
        public static bool IsHandFlipped(DynFamilyInstance familyInstance)
        {
            if (familyInstance.InternalElement is DB.FamilyInstance revitFamilyInstance)
            {
                int handFlipped = revitFamilyInstance.HandFlipped ? 1 : 0;
                int faceFlipped = revitFamilyInstance.FacingFlipped ? 1 : 0;
                return handFlipped + faceFlipped == 1;
            }
            return false;
        }

        /// <summary>
        /// Returns the Family of the FamilyInstance.
        /// </summary>
        /// <param name="familyInstance">The FamilyInstance.</param>
        /// <returns name="family">The Family of the FamilyInstance.</returns>
        /// <search>Revit.FamilyInstance.Family</search>
        [NodeCategory("Query")]
        public static DynElement? Family(DynFamilyInstance familyInstance)
        {
            if (familyInstance.InternalElement is DB.FamilyInstance revitFamilyInstance)
            {
                return revitFamilyInstance.Symbol.Family.Ext_ToDynElement(true);
            }
            return null;
        }
    }
}