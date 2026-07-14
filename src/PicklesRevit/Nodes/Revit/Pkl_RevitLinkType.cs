namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to RevitLinkTypes.
    /// </summary>
    public class Pkl_RevitLinkType
    {
        internal Pkl_RevitLinkType() { }

        /// <summary>
        /// Gets all RevitLinkInstances of a given RevitLinkType.
        /// </summary>
        /// <param name="revitLinkType">Type to get all instances of.</param>
        /// <returns name="revitLinkInstances">All instances of the RevitLinkType.</returns>
        /// <search>Revit.RevitLinkInstance.GetAllInstances</search>
        [NodeCategory("Action")]
        public static IList<DynElement> GetAllInstances(DynElement revitLinkType)
        {
            if (revitLinkType.InternalElement is DB.RevitLinkType dbLinkType)
            {
                DB.ElementId linkTypeId = dbLinkType.Id;

                return dbLinkType.Document.Ext_Collector()
                        .OfClass(typeof(DB.RevitLinkInstance))
                        .Cast<DB.RevitLinkInstance>()
                        .Where(i => i.GetTypeId() == linkTypeId)
                        .Ext_ToDynamoElements(true);
            }
            return new List<DynElement>();
        }

        /// <summary>
        /// Returns if the RevitLinkType is loaded in its document.
        /// </summary>
        /// <param name="revitLinkType">RevitLinkType to check.</param>
        /// <returns name="isLoaded">If the RevitLinkType is loaded.</returns>
        /// <search>Revit.RevitLinkType.IsLoaded</search>
        [NodeCategory("Query")]
        public static bool IsLoaded(DynElement revitLinkType)
        {
            if (revitLinkType.InternalElement is DB.RevitLinkType dbLinkType)
            {
                return DB.RevitLinkType.IsLoaded(dbLinkType.Document, dbLinkType.Id);
            }
            return false;
        }
    }
}