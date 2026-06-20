using System.Reflection;

namespace Pickles.Extensions
{
    internal static class Ext_DynElements
    {
        internal static DB.ForgeTypeId Ext_ToSpecTypeId(this DynSpecType specType)
        {
            return new DB.ForgeTypeId(specType.TypeId);
        }

        internal static DB.ForgeTypeId Ext_ToGroupTypeId(this DynGroupType groupType)
        {
            return new DB.ForgeTypeId(groupType.TypeId);
        }

        internal static DB.ForgeTypeId Ext_ToForgeTypeId(this DynForgeType forgeType)
        {
            return new DB.ForgeTypeId(forgeType.TypeId);
        }

        /// <summary>
        /// Attempts to convert a Dynamo Document to the Revit type.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="doc">The Dynamo Document.</param>
        /// <returns>A Revit DB Document.</returns>
        internal static DB.Document? Ext_ToDBDocument(this DynDocument doc)
        {
            if (doc is null) { return null; }

            var property = typeof(DynDocument).GetProperty("InternalDocument",
                BindingFlags.NonPublic | BindingFlags.Instance);

            return property.GetValue(doc) as DB.Document;
        }

        /// <summary>
        /// Attempts to convert a Dynamo Warning to the Revit type.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="warning"></param>
        /// <returns>A Revit DB FailureMessage.</returns>
        internal static DB.FailureMessage? Ext_ToFailureMessage(this DynWarning warning)
        {
            if (warning is null) { return null; }

            var property = typeof(DynWarning).GetProperty("InternalWarning",
                BindingFlags.NonPublic | BindingFlags.Instance);

            return property.GetValue(warning) as DB.FailureMessage;
        }
    }
}
