namespace Pickles.Extensions
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

        /// <summary>
        /// Collects DB ViewSheets, with an optional Sheet Collection Id.
        /// </summary>
        /// <param name="doc">The DB Document.</param>
        /// <param name="sheetCollectionId">The Id of the colletion. Null = all sheets, Invalid = no collection.</param>
        /// <param name="includePlaceholders">Include placeholder Sheets.</param>
        /// <returns>An IList of DB ViewSheets.</returns>
        internal static IList<DB.ViewSheet> Ext_CollectSheets(this DB.Document doc,
            DB.ElementId sheetCollectionId = null, bool includePlaceholders = true)
        {
            var sheets = new List<DB.ViewSheet>();

            IEnumerable<DB.ViewSheet> allSheets = doc.Ext_CollectByClass<DB.ViewSheet>();

            if (!includePlaceholders)
            {
                allSheets = allSheets.Where(s => !s.IsPlaceholder);
            }

            foreach (DB.ViewSheet sheet in doc.Ext_CollectByClass<DB.ViewSheet>())
            {
                if (sheetCollectionId == null
                    || sheet.SheetCollectionId == sheetCollectionId)
                {
                    sheets.Add(sheet);
                }
            }

            return sheets;
        }

        /// <summary>
        /// Collects DB ViewSheets into a dictionary keyed by their number, with an optional Sheet Collection Id.
        /// </summary>
        /// <param name="doc">The DB Document.</param>
        /// <param name="sheetCollectionId">The Id of the colletion. Null = all sheets, Invalid = no collection.</param>
        /// <param name="includePlaceholders">Include placeholder Sheets.</param>
        /// <returns>An IDictionary of DB ViewSheets keyed by SheetNumber.</returns>
        internal static IDictionary<string, DB.ViewSheet> Ext_CollectSheetsByNumber(this DB.Document doc,
            DB.ElementId sheetCollectionId = null, bool includePlaceholders = true)
        {
            var outDictionary = new Dictionary<string, DB.ViewSheet>();

            foreach (DB.ViewSheet sheet in doc.Ext_CollectSheets(sheetCollectionId, includePlaceholders))
            {
                // NB if all sheets, will always be last sheet of that number
                outDictionary[sheet.SheetNumber] = sheet;
            }

            return outDictionary;
        }

        /// <summary>
        /// Creates a DB ViewSheet with provided values.
        /// </summary>
        /// <param name="doc">Related Document.</param>
        /// <param name="number">Sheet Number to apply.</param>
        /// <param name="name">Sheet Name to apply.</param>
        /// <param name="asPlaceholder">If the sheet is a placeholder.</param>
        /// <param name="ttbId">The ElementId of the Titleblock FamilyType to use.</param>
        /// <returns>A DB ViewSheet.</returns>
        internal static DB.ViewSheet Ext_CreateSheet(this DB.Document doc, string number, string name,
            bool asPlaceholder, DB.ElementId ttbId)
        {
            DB.ViewSheet sheet;

            if (asPlaceholder)
            {
                sheet = DB.ViewSheet.CreatePlaceholder(doc);
            }
            else
            {
                sheet = DB.ViewSheet.Create(doc, ttbId);
            }

            sheet.SheetNumber = number;
            sheet.Name = name;

            return sheet;
        }
    }
}
