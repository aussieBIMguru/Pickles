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

        internal static DB.View? Ext_ToRevitView(this DynView view)
        {
            if (view?.InternalElement is DB.View revitView)
            {
                return revitView;
            }
            return null;
        }

        internal static DynPoint Ext_ToDynamoPoint(this DB.XYZ point, bool convertToProject = false)
        {
            if (convertToProject)
            {
                return DynPoint.ByCoordinates(point.X.Ext_InternalToProject(DB.SpecTypeId.Length),
                    point.Y.Ext_InternalToProject(DB.SpecTypeId.Length),
                    point.Z.Ext_InternalToProject(DB.SpecTypeId.Length));
            }
            else
            {
                return DynPoint.ByCoordinates(point.X, point.Y, point.Z);
            }
        }

        internal static DynVector Ext_ToDynamoVector(this DB.XYZ vector)
        {
            return DynVector.ByCoordinates(vector.X, vector.Y, vector.Z);
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

        internal static DB.BuiltInCategory? Ext_ToBuiltInCategory(this DynCategory category)
        {
            if (category == null)
                return null;

            return (DB.BuiltInCategory)category.Id;
        }
    }
}
