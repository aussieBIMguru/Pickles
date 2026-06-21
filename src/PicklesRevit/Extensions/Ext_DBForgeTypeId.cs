using System.Reflection;

namespace Pickles.Extensions
{
    internal static class Ext_DBForgeTypeId
    {
        /// <summary>
        /// Attempts to convert a Dynamo ForgeType to a DB ForgeTypeId.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="forgeTypeId">The Revit DB ForgeTypeId.</param>
        /// <returns>A Dynamo ForgeType.</returns>
        internal static DynForgeType? Ext_ToDynForgeType(this DB.ForgeTypeId forgeTypeId)
        {
            if (forgeTypeId is null) return null;

            var constructor = typeof(DynForgeType).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(DB.ForgeTypeId) },
                null);

            return constructor?.Invoke(new object[] { forgeTypeId }) as DynForgeType;
        }

        /// <summary>
        /// Attempts to convert a Dynamo GroupType to a DB GroupType.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="groupType">The Revit DB GroupType.</param>
        /// <returns>A Dynamo GroupType.</returns>
        internal static DynGroupType? Ext_ToDynGroupType(this DB.ForgeTypeId groupType)
        {
            if (groupType is null) return null;

            var constructor = typeof(DynGroupType).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(DB.ForgeTypeId) },
                null);

            return constructor?.Invoke(new object[] { groupType }) as DynGroupType;
        }

        /// <summary>
        /// Attempts to convert a Dynamo SpecType to a DB SpecType.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="specType">The Revit DB SpecType.</param>
        /// <returns>A Dynamo SpecType.</returns>
        internal static DynSpecType? Ext_ToDynSpecType(this DB.ForgeTypeId specType)
        {
            if (specType is null) return null;

            var constructor = typeof(DynSpecType).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(DB.ForgeTypeId) },
                null);

            return constructor?.Invoke(new object[] { specType }) as DynSpecType;
        }
    }
}
