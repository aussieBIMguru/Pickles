using System.Reflection;

namespace Pickles.Extensions
{
    internal static class Ext_DBScheduleField
    {
        /// <summary>
        /// Attempts to convert a Revit ScheduleField to the Dynamo type.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="field">The Revit DB ScheduleField.</param>
        /// <returns>A Dynamo ScheduleField.</returns>
        internal static DynScheduleField? Ext_ToDynScheduleField(this DB.ScheduleField field)
        {
            if (field is null)
            {
                return null;
            }

            var constructor = typeof(DynScheduleField)
                .GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(DB.ScheduleField) },
                    null);

            return constructor?.Invoke(new object[] { field }) as DynScheduleField;
        }
    }
}
