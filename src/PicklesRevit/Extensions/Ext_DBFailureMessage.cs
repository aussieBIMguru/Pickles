using System.Reflection;

namespace Pickles.Extensions
{
    internal static class Ext_DBFailureMessage
    {
        /// <summary>
        /// Attempts to convert a Revit Warnng to the Dynamo type.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="warning">The Revit DB FailureMessage.</param>
        /// <returns>A Dynamo Warning.</returns>
        internal static DynWarning? Ext_ToDynWarning(this DB.FailureMessage warning)
        {
            if (warning is null) { return null; }

            var constructor = typeof(DynWarning).GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

            return constructor.Invoke(new object[] { warning }) as DynWarning;
        }
    }
}
