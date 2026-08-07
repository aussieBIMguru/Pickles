namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to GroupTypes.
    /// </summary>
    public class Pkl_GroupType
    {
        internal Pkl_GroupType() { }

        /// <summary>
        /// Gets GroupType by provided name, or an invalid type if not found.
        /// </summary>
        /// <param name="name">The name to get.</param>
        /// <returns name="groupType">The GroupType that was found.</returns>
        /// <search>Revit.GroupType.ByName</search>
        [NodeCategory("Action")]
        public static DynGroupType? ByName(string name)
        {
            List<DynGroupType?> groupTypes = new();

            foreach (var groupType in DB.ParameterUtils.GetAllBuiltInGroups())
            {
                if (name == DB.LabelUtils.GetLabelForGroup(groupType))
                {
                    return groupType.Ext_ToDynGroupType();
                }
            }

            return null;
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