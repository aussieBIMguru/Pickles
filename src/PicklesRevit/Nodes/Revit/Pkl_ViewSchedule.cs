using Autodesk.Revit.DB;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Schedule Views.
    /// </summary>
    public class Pkl_ViewSchedule
    {
        internal Pkl_ViewSchedule() { }

        /// <summary>
        /// Sets the column headings of Schedule Fields to their parameter names.
        /// </summary>
        /// <param name="schedule">The Schedule View.</param>
        /// <returns name="names">The names of the Schedule Fields.</returns>
        /// <search>Revit.ViewSchedule.ResetHeadings</search>
        [NodeCategory("Action")]
        public static List<string> ResetHeadings(DynViewSchedule schedule)
        {
            // Get the underlying Revit Schedule
            if (schedule.InternalElement is not DB.ViewSchedule dbSchedule)
            {
                return [];
            }

            // Get the Schedule Definition
            var definition = dbSchedule.Definition;

            // Store the field names for output
            var names = new List<string>();

            // Reset each Schedule Field
            dbSchedule.Document.Ext_EnsureTransaction();

            for (int i = 0; i < definition.GetFieldCount(); i++)
            {
                var field = definition.GetField(i);
                string name = field.GetName();
                field.ColumnHeading = name;
                names.Add(name);
            }

            dbSchedule.Document.Ext_TransactionDone();

            return names;
        }

        /// <summary>
        /// Returns the displayed text of all cells in the body section of a Schedule.
        /// </summary>
        /// <param name="schedule">The Schedule View.</param>
        /// <returns name="data">The schedule data as rows and columns.</returns>
        /// <search>Revit.ViewSchedule.GetDataAsText</search>
        [NodeCategory("Action")]
        public static List<List<string>> GetDataAsText(DynViewSchedule schedule)
        {
            // Get the underlying Revit Schedule
            if (schedule.InternalElement is not DB.ViewSchedule dbSchedule)
            {
                return [];
            }

            // Get the table data for the schedule
            DB.TableData tableData = dbSchedule.GetTableData();
            DB.TableSectionData sectionData = tableData.GetSectionData(DB.SectionType.Body);

            // Get the number of rows and columns in the body
            int numberOfRows = sectionData.NumberOfRows;
            int numberOfColumns = sectionData.NumberOfColumns;

            // Create the output data
            List<List<string>> data = new();

            // Iterate through each row
            for (int row = 0; row < numberOfRows; row++)
            {
                var rowData = new List<string>();

                // Iterate through each column in the row
                for (int column = 0; column < numberOfColumns; column++)
                {
                    // Get the displayed text for the cell
                    rowData.Add(
                        dbSchedule.GetCellText(
                            DB.SectionType.Body,
                            row,
                            column));
                }

                data.Add(rowData);
            }

            return data;
        }

        /// <summary>
        /// Returns the keys of a Key Schedule.
        /// </summary>
        /// <param name="schedule">The Key Schedule.</param>
        /// <returns name="keys">The keys of the schedule.</returns>
        /// <search>Revit.ViewSchedule.Keys</search>
        [NodeCategory("Query")]
        public static IList<DynElement?> Keys(DynViewSchedule schedule)
        {
            if (schedule.InternalElement is not DB.ViewSchedule dbSchedule
                || !dbSchedule.Definition.IsKeySchedule)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Schedule is not a Key Schedule.");
                return [];
            }

            // Collect and return Keys
            return dbSchedule.Document.Ext_Collector(view: dbSchedule)
                .Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Returns the fields of the Schedule.
        /// </summary>
        /// <param name="schedule">The Schedule View.</param>
        /// <returns name="fields">The Schedule Fields of the schedule.</returns>
        /// <search>Revit.ViewSchedule.GetField</search>
        [NodeCategory("Query")]
        public static List<DynScheduleField?> Fields(DynViewSchedule schedule)
        {
            if (schedule.InternalElement is DB.ViewSchedule dbSchedule)
            {
                var definition = dbSchedule.Definition;

                return Enumerable.Range(0, definition.GetFieldCount())
                    .Select(i => definition.GetField(i))
                    .Select(f => f.Ext_ToDynScheduleField())
                    .Where(f => f is not null)
                    .ToList();
            }

            return [];
        }

        /// <summary>
        /// Returns if a Schedule is a Revision Schedule.
        /// Note that this runs a simple Name check, so will only work in English.
        /// </summary>
        /// <param name="schedule">The Schedule View.</param>
        /// <returns name="isRevisionSchedule">If the Schedule is a Revision Schedule.</returns>
        /// <search>Revit.ViewSchedule.IsRevisionSchedule</search>
        [NodeCategory("Query")]
        public static bool IsRevisionSchedule(DynViewSchedule schedule)
        {
            if (schedule.InternalElement is DB.ViewSchedule dbSchedule)
            {
                return dbSchedule.Name.Contains("<Revision Schedule>",
                    StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Returns if a Schedule is a Key Schedule.
        /// </summary>
        /// <param name="schedule">The Schedule View.</param>
        /// <returns name="isKeySchedule">If the Schedule is a Key Schedule.</returns>
        /// <search>Revit.ViewSchedule.IsKeySchedule</search>
        [NodeCategory("Query")]
        public static bool IsKeySchedule(DynViewSchedule schedule)
        {
            if (schedule.InternalElement is DB.ViewSchedule dbSchedule)
            {
                return dbSchedule.Definition.IsKeySchedule;
            }

            return false;
        }
    }
}