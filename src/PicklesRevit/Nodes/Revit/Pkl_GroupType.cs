namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to GroupTypes.
    /// </summary>
    public class Pkl_GroupType
    {
        internal Pkl_GroupType() { }

        /// <summary>
        /// Gets GroupTypes by provided names, or an invalid type if not found.
        /// </summary>
        /// <param name="names">The names to get.</param>
        /// <returns name="groupTypes">The GroupTypes that were found.</returns>
        /// <search>Revit.GroupType.ByNames</search>
        [NodeCategory("Action")]
        public static List<DynGroupType?> ByNames(List<string> names)
        {
            // Output list
            List<DynGroupType?> groupTypes = new();

            // Null guard inputs
            if (names == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return groupTypes;
            }

            // Group type dictionary
            var groupTypeDictionary = DB.ParameterUtils.GetAllBuiltInGroups()
                .GroupBy(g => DB.LabelUtils.GetLabelForGroup(g))
                .ToDictionary(g => g.Key, g => g.First());

            // Get the group types
            foreach (var name in names)
            {
                if (groupTypeDictionary.TryGetValue(name, out var groupType))
                {
                    groupTypes.Add(groupType.Ext_ToDynGroupType());
                }
                else
                {
                    groupTypes.Add(new DB.ForgeTypeId().Ext_ToDynGroupType());
                }
            }

            // Return output
            return groupTypes;
        }

        /// <summary>
        /// Gets the name of the provided GroupType.
        /// </summary>
        /// <param name="groupType">The GroupType.</param>
        /// <returns name="name">The GroupType's name.</returns>
        /// <search>Revit.GroupType.Name</search>
        [NodeCategory("Query")]
        public static string Name(DynGroupType groupType)
        {
            return DB.LabelUtils.GetLabelForGroup(groupType.Ext_ToGroupTypeId());
        }
    }
}