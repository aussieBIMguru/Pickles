using Pickles.Extensions;
using Revit.Elements;

namespace Pickles.Extensions
{
    /// <summary>
    /// Extension methods for DB Elements.
    /// </summary>
    internal static class Ext_DBElement
    {
        /// <summary>
        /// Converts a Revit element to a Dynamo element, accounting for null guarding.
        /// </summary>
        /// <param name="element">The Element to convert.</param>
        /// <param name="revitOwned">If the Element is Revit owned.</param>
        /// <returns>A Dynamo Element.</returns>
        internal static DynElement? Ext_ToDynElement(this DB.Element element, bool revitOwned)
        {
            return element == null ? null : element.ToDSType(revitOwned);
        }

        /// <summary>
        /// Retrieve a parameter from an element via a builtin parameter.
        /// </summary>
        /// <param name="element">The Element (extended).</param>
        /// <param name="builtInParameter">A Revit BuiltInParameter.</param>
        /// <returns>A Parameter object.</returns>
        internal static DB.Parameter? Ext_GetBuiltInParameter(this DB.Element element, DB.BuiltInParameter builtInParameter)
        {
            if (element is null || builtInParameter == DB.BuiltInParameter.INVALID) { return null; }

            DB.ForgeTypeId ftid = DB.ParameterUtils.GetParameterTypeId(builtInParameter);
            return element.GetParameter(ftid);
        }

        internal static DB.ElementId? Ext_ToSheetCollectionId(this DB.Element element)
        {
            // Case 1 - Actual sheet collection
            if (element is DB.SheetCollection internalSheetCollection)
            {
                return internalSheetCollection.Id;
            }
            // Case 2 - Flag no collection
            else if (element != null)
            {
                return DB.ElementId.InvalidElementId;
            }
            // Case 3 - Flag all sheets (element is null)
            else
            {
                return null;
            }
        }
    }
}
