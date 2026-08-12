using Autodesk.Revit.DB;
using System.Reflection;

namespace Pickles.Extensions
{
    /// <summary>
    /// Extension methods for DB FamilyParameters.
    /// </summary>
    internal static class Ext_DBFamilyParameter
    {
        /// <summary>
        /// Attempts to convert a Revit FamilyParameter to the Dynamo type.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="parameter">The Revit DB FamilyParameter.</param>
        /// <returns>A Dynamo FamilyParameter.</returns>
        internal static DynFamilyParameter? Ext_ToDynFamilyParameter(this DB.FamilyParameter parameter)
        {
            if (parameter is null)
            {
                return null;
            }

            var constructor = typeof(DynFamilyParameter)
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(DB.FamilyParameter) },
                    null);

            return constructor?.Invoke(new object[] { parameter }) as DynFamilyParameter;
        }

        /// <summary>
        /// Returns if a Parameter is of the YesNo type.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>If the parameter is a YesNo type.</returns>
        internal static bool Ext_IsYesNo(this DB.FamilyParameter parameter)
        {
            return parameter.Definition.GetDataType() == DB.SpecTypeId.Boolean.YesNo;
        }
    }
}
