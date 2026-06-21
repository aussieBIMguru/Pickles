namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to SpecTypes.
    /// </summary>
    public class Pkl_SpecType
    {
        internal Pkl_SpecType() { }

        /// <summary>
        /// Gets SpecTypes by provided names, or an invalid type if not found.
        /// </summary>
        /// <param name="names">The names to get.</param>
        /// <returns name="specTypes">The SpecTypes that were found.</returns>
        /// <search>Revit.SpecType.ByNames</search>
        [NodeCategory("Action")]
        public static List<DynSpecType?> ByNames(List<string> names)
        {
            // Output list
            List<DynSpecType?> specTypes = new();

            // Null guard inputs
            if (names == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return specTypes;
            }

            // Spec type dictionary
            var specTypeDictionary = DB.SpecUtils.GetAllSpecs()
                .GroupBy(s => DB.LabelUtils.GetLabelForSpec(s))
                .ToDictionary(s => s.Key, s => s.First());

            // Get the Spec types
            foreach (var name in names)
            {
                if (specTypeDictionary.TryGetValue(name, out var specType))
                {
                    specTypes.Add(specType.Ext_ToDynSpecType());
                }
                else
                {
                    specTypes.Add(new DB.ForgeTypeId().Ext_ToDynSpecType());
                }
            }

            // Return output
            return specTypes;
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