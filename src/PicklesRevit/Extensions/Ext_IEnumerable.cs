namespace Pickles.Extensions
{
    /// <summary>
    /// Extension methods for IEnumerable objects.
    /// </summary>
    internal static class Ext_IEnumerable
    {
        /// <summary>
        /// Converts a list of DB Elements to Dynamo Elements.
        /// </summary>
        /// <param name="revitElements">Elements to convert.</param>
        /// <param name="revitOwned">Does Revit own the Elements.</param>
        /// <returns>A list of Dynamo Elements.</returns>
        internal static IList<DynElement?> Ext_ToDynamoElements<T>(this IEnumerable<T> revitElements, bool revitOwned)
            where T : DB.Element
        {
            return revitElements
                .Select(e => e.Ext_ToDynElement(revitOwned))
                .ToList();
        }

        /// <summary>
        /// Converts a list of DB ElementIds to Dynamo Elements.
        /// </summary>
        /// <param name="elementIds">ElementIds to convert.</param>
        /// <param name="revitOwned">Does Revit own the Elements.</param>
        /// <param name="doc">Document to get Elements from.</param>
        /// <returns>A list of Dynamo Elements.</returns>
        internal static IList<DynElement?> Ext_ToDynamoElements(this IEnumerable<DB.ElementId> elementIds,
            bool revitOwned, DB.Document doc)
        {
            return elementIds
                .Select(i => doc.GetElement(i))
                .Select(e => e.Ext_ToDynElement(revitOwned))
                .ToList();
        }

        /// <summary>
        /// Checks for invalid List or contents.
        /// </summary>
        /// <typeparam name="T">The object type the list holds (implied).</typeparam>
        /// <param name="list">The list to review.</param>
        /// <param name="ensureNoNulls">Ensure there are no nulls in the list.</param>
        /// <returns>A Boolean.</returns>
        internal static bool Ext_ListIsValid<T>(this IEnumerable<T> list, bool ensureNoNulls = false)
        {
            if (ensureNoNulls)
            {
                return list == null || list.Count() == 0 || list.Any(i => i is null);
            }
            else
            {
                return list == null || list.Count() == 0;
            }
        }
    }
}
