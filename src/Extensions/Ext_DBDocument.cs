namespace Pkl_Extensions
{
    /// <summary>
    /// Extension methods for DB Documents.
    /// </summary>
    internal static class Ext_DBDocument
    {
        /// <summary>
        /// Creates a collector with optional view and choice of elements of element types.
        /// </summary>
        /// <param name="doc">The DB Document.</param>
        /// <param name="elementTypes">Collect ElementTypes.</param>
        /// <param name="view">Optional view to limit collection to.</param>
        /// <returns>A DB FilteredElementCollector.</returns>
        internal static DB.FilteredElementCollector Ext_Collector(this DB.Document doc, bool elementTypes = false, DB.View view = null)
        {
            // Catch nulls
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc), "Document cannot be null.");
            }

            // Instantiate with optional view
            DB.FilteredElementCollector collector = view == null
                ? new DB.FilteredElementCollector(doc)
                : new DB.FilteredElementCollector(doc, view.Id);

            // Handle element / element type
            return elementTypes
                ? collector.WhereElementIsElementType()
                : collector.WhereElementIsNotElementType();
        }

        /// <summary>
        /// Collects DB Elements by BuiltInCategory.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <param name="doc">The DB Document.</param>
        /// <param name="elementTypes">Collect ElementTypes.</param>
        /// <param name="view">Optional view to limit collection to.</param>
        /// <returns>An IList of DB Elements.</returns>
        internal static IList<DB.Element> Ext_CollectByCategory(this DB.Document doc,
            DB.BuiltInCategory category, bool elementTypes = false, DB.View view = null)
        {
            // Catch nulls / invalid Category
            if (doc == null || category == DB.BuiltInCategory.INVALID)
            {
                return new List<DB.Element>();
            }

            // Return DB Elements by category
            return doc.Ext_Collector(elementTypes: elementTypes, view: view)
                .OfCategory(category)
                .ToElements();
        }

        /// <summary>
        /// Collects DB Elements by BuiltInCategory, then converts them to Dynamo Elements.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <param name="doc">The DB Document.</param>
        /// <param name="elementTypes">Collect ElementTypes.</param>
        /// <param name="view">Optional view to limit collection to.</param>
        /// <param name="revitOwned">If the Dynamo Elements are Revit owned.</param>
        /// <returns>An IList of Dynamo Elements.</returns>
        internal static IList<DynElement?> Ext_CollectByCategoryToDyn(this DB.Document doc,
            DB.BuiltInCategory category, bool elementTypes = false, DB.View view = null, bool revitOwned = true)
        {
            return doc.Ext_CollectByCategory(category, elementTypes: elementTypes, view: view)
                .Select(e => e.Ext_ToDynElement(revitOwned))
                .ToList();
        }

        /// <summary>
        /// Collects DB Elements by Class.
        /// </summary>
        /// <typeparam name="T">The DB Class to filter by.</typeparam>
        /// <param name="doc">The DB Document.</param>
        /// <param name="elementTypes">Collect ElementTypes.</param>
        /// <param name="view">Optional view to limit collection to.</param>
        /// <returns>An IList of DB Elements.</returns>
        internal static IList<T> Ext_CollectByClass<T>(this DB.Document doc,
            bool elementTypes = false, DB.View view = null)
        {
            if (doc == null)
            {
                return new List<T>();
            }

            return doc.Ext_Collector(elementTypes: elementTypes, view: view)
                .OfClass(typeof(T))
                .Cast<T>()
                .ToList();
        }

        /// <summary>
        /// Collects DB Elements by Class, then converts them to Dynamo Elements.
        /// </summary>
        /// <typeparam name="T">The DB Class to filter by.</typeparam>
        /// <param name="doc">The DB Document.</param>
        /// <param name="elementTypes">Collect ElementTypes.</param>
        /// <param name="view">Optional view to limit collection to.</param>
        /// <param name="revitOwned">If the Dynamo Elements are Revit owned.</param>
        /// <returns>An IList of Dynamo Elements.</returns>
        internal static IList<DynElement?> Ext_CollectByClassToDyn<T>(this DB.Document doc,
            bool elementTypes = false, DB.View view = null, bool revitOwned = true)
        {
            return doc.Ext_CollectByClass<T>(elementTypes: elementTypes, view: view)
                .OfType<DB.Element>()
                .Select(e => e.Ext_ToDynElement(revitOwned))
                .ToList();
        }
    }
}
