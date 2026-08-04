namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to FamilyTypes.
    /// </summary>
    public class Pkl_FamilyType
    {
        internal Pkl_FamilyType() { }

        /// <summary>
        /// Gets the name of a FamilyType.
        /// </summary>
        /// <param name="familyType">The FamilyType.</param>
        /// <returns name="name">The name of the FamilyType.</returns>
        /// <search>Revit.FamilyType.Name</search>
        [NodeCategory("Query")]
        public static string Name(DB.FamilyType familyType)
        {
            return familyType.Name;
        }
    }
}
