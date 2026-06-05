namespace Pickle.Extensions
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
        internal static IList<DynElement?> Ext_ToDynamoElements(this IEnumerable<DB.Element> revitElements, bool revitOwned = false)
        {
            return revitElements
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
