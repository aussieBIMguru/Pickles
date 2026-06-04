using Revit.Elements;

namespace Pkl_Extensions
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
    }
}
