namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to SpecTypes.
    /// </summary>
    public class Pkl_SpecType
    {
        internal Pkl_SpecType() { }

        /// <summary>
        /// Gets SpecTypes by provided name, or an invalid type if not found.
        /// </summary>
        /// <param name="name">The name to get.</param>
        /// <returns name="specType">The SpecType that was found.</returns>
        /// <search>Revit.SpecType.ByName</search>
        [NodeCategory("Action")]
        public static DynSpecType? ByNames(string name)
        {
            foreach (var specType in DB.SpecUtils.GetAllSpecs())
            {
                if (name == DB.LabelUtils.GetLabelForSpec(specType))
                {
                    return specType.Ext_ToDynSpecType();
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the name of the provided SpecType.
        /// </summary>
        /// <param name="specType">The SpecType.</param>
        /// <returns name="name">The SpecType's name.</returns>
        /// <search>Revit.SpecType.Name</search>
        [NodeCategory("Query")]
        public static string Name(DynSpecType specType)
        {
            return DB.LabelUtils.GetLabelForSpec(specType.Ext_ToSpecTypeId());
        }
    }
}