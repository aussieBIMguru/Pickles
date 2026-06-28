namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to FamilyInstances.
    /// </summary>
    public class Pkl_FamilyInstance
    {
        internal Pkl_FamilyInstance() { }

        /// <summary>
        /// Returns if the FamilyInstance is hand flipped.
        /// </summary>
        /// <param name="element">The Element.</param>
        /// <returns name="handFlipped">If the Element is hand flipped.</returns>
        /// <search>Revit.FamilyInstance.IsHandFlipped</search>
        [NodeCategory("Query")]
        public static bool IsHandFlipped(DynElement element)
        {
            if (element.InternalElement is DB.FamilyInstance familyInstance)
            {
                int handFlipped = familyInstance.HandFlipped ? 1 : 0;
                int faceFlipped = familyInstance.FacingFlipped ? 1 : 0;
                return handFlipped + faceFlipped == 1;
            }
            return false;
        }
    }
}