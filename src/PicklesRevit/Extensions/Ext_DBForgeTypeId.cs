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

            return constructor.Invoke(new object[] { forgeTypeId }) as DynForgeType;
        }
    }
}
