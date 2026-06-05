namespace Pickle.Extensions
{
    /// <summary>
    /// Extension methods for DB ElementIds.
    /// </summary>
    internal static class Ext_DBElementId
    {
        /// <summary>
        /// Returns if an ElementId is not null or invalid.
        /// </summary>
        /// <param name="id">The ElementId to check.</param>
        /// <returns>A Boolean.</returns>
        public static bool Ext_IsValid(this DB.ElementId id)
        {
            return id != null && id != DB.ElementId.InvalidElementId;
        }

        /// <summary>
        /// Returns if an ElementId is null or invalid.
        /// </summary>
        /// <param name="id">The ElementId to check.</param>
        /// <returns>A Boolean.</returns>
        public static bool Ext_IsInValid(this DB.ElementId id)
        {
            return id == null || id == DB.ElementId.InvalidElementId;
        }

        /// <summary>
        /// Gets the Element of a given ElementId if it's of the specified type.
        /// </summary>
        /// <typeparam name="T">The Type to check for.</typeparam>
        /// <param name="id">The ElementId to convert to an Element.</param>
        /// <param name="doc">The Document to get the Element from.</param>
        /// <returns>A DB Element.</returns>
        public static T? Ext_GetElement<T>(this DB.ElementId id, DB.Document doc) where T: DB.Element
        {
            if (id.Ext_IsValid() && doc != null
                && doc.GetElement(id) is T t)
            {
                return t;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Gets the Dynamo Element of a given ElementId if it's of the specified type.
        /// </summary>
        /// <param name="id">The ElementId to convert to an Element.</param>
        /// <param name="doc">The Document to get the Element from.</param>
        /// <param name="revitOwned">If the Element is Revit owned.</param>
        /// <returns>A Dynamo Element.</returns>
        public static DynElement? Ext_GetDynamoElement(this DB.ElementId id, DB.Document doc, bool revitOwned)
        {
            return id.Ext_GetElement<DB.Element>(doc).Ext_ToDynElement(revitOwned);
        }
    }
}
