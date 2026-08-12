namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Groups.
    /// </summary>
    public class Pkl_Group
    {
        internal Pkl_Group() { }

        /// <summary>
        /// Gets all Elements in a Group.
        /// </summary>
        /// <param name="group">The Group.</param>
        /// <returns name="elements">Elements in the Group.</returns>
        /// <search>Revit.Group.GetElements</search>
        [NodeCategory("Action")]
        public static List<DynElement> GetElements(DynGroup group)
        {
            if (group.InternalElement is DB.Group revitGroup)
            {
                DB.Document doc = revitGroup.Document;
                return revitGroup.GetMemberIds()
                    .Select(i => i.Ext_GetDynamoElement(doc, true))
                    .ToList();
            }
            else
            {
                return new List<DynElement>();
            }
        }

        /// <summary>
        /// Gets the GroupType of a Group.
        /// </summary>
        /// <param name="group">The Group.</param>
        /// <returns name="groupType">The Group's type.</returns>
        /// <search>Revit.Group.GroupType</search>
        [NodeCategory("Query")]
        public static DynElement? GroupType(DynGroup group)
        {
            if (group.InternalElement is DB.Group revitGroup)
            {
                return revitGroup.GroupType.Ext_ToDynElement(true);
            }
            else
            {
                return null;
            }
        }
    }
}