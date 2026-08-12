namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Schedule Fields.
    /// </summary>
    public class Pkl_ScheduleField
    {
        internal Pkl_ScheduleField() { }

        /// <summary>
        /// Returns the actual name of a Schedule Field.
        /// </summary>
        /// <param name="field">The Schedule Field.</param>
        /// <returns name="name">The name of the Field</returns>
        /// <search>Revit.ScheduleField.Name</search>
        [NodeCategory("Query")]
        public static string? Name(DynScheduleField field)
        {
            if (field.Ext_ToSchedulableField() is DB.ScheduleField dbField)
            {
                return dbField.GetName();
            }

            return null;
        }

        /// <summary>
        /// Returns the Column Heading of a Schedule Field.
        /// </summary>
        /// <param name="field">The Schedule Field.</param>
        /// <returns name="name">The Heading of the Field's column.</returns>
        /// <search>Revit.ScheduleField.ColumnHeading</search>
        [NodeCategory("Query")]
        public static string? ColumnHeading(DynScheduleField field)
        {
            if (field.Ext_ToSchedulableField() is DB.ScheduleField dbField)
            {
                return dbField.ColumnHeading;
            }

            return null;
        }
    }
}