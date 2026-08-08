using System.Reflection.Metadata;

namespace Pickles.Extensions
{
    internal static class Ext_DBFamilyManager
    {
        /// <summary>
        /// Gets a Parameter by name.
        /// </summary>
        /// <param name="fm">The Revit DB FamilyManager.</param>
        /// <param name="parameterName">The name of the Parameter to find.</param>
        /// <returns>A DB.FamilyParameter (null if not found).</returns>
        internal static DB.FamilyParameter? Ext_GetParameterByName(this DB.FamilyManager fm,
            string parameterName)
        {
            // Get parmeter if found
            return fm.Parameters
                .Cast<DB.FamilyParameter>()
                .FirstOrDefault(p => p.Definition.Name == parameterName);
        }

        /// <summary>
        /// Gets a Type by name.
        /// </summary>
        /// <param name="fm">The Revit DB FamilyManager.</param>
        /// <param name="typeName)">The name of the Type to find.</param>
        /// <returns>A DB.FamilyType (null if not found).</returns>
        internal static DB.FamilyType? Ext_GetTypeByName(this DB.FamilyManager fm,
            string typeName)
        {
            // Get parmeter if found
            return fm.Types
                .Cast<DB.FamilyType>()
                .FirstOrDefault(t => t.Name == typeName);
        }

        /// <summary>
        /// Sets the current FamilyType if not current and able.
        /// </summary>
        /// <param name="fm">The Revit DB FamilyManager.</param>
        /// <param name="familyType">The FamilyType to set (by name).</param>
        /// <returns>If the type was set (or already current).</returns>
        internal static bool Ext_SetCurrentType(this DB.FamilyManager fm,
            DB.FamilyType familyType)
        {
            // Invalid family type, return null
            if (familyType == null) { return false; }
            
            // If type is already set, don't need to set
            if (fm.CurrentType?.Name == familyType.Name)
            {
                return true;
            }
            
            // Get current type with same name
            DB.FamilyType? setType = fm.Types
                .Cast<DB.FamilyType>()
                .FirstOrDefault(t => t.Name == familyType?.Name);

            // Set the current type if found
            if (setType != null)
            {
                fm.CurrentType = setType;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
