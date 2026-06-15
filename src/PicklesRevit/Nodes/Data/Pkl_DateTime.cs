using System.Globalization;

namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to DateTimes.
    /// </summary>
    public class Pkl_DateTime
    {
        internal Pkl_DateTime() { }

        /// <summary>
        /// Returns the system DateTime for now.
        /// </summary>
        /// <param name="refresh">Update the node contents.</param>
        /// <returns name="dateTime">A System.DateTime.</returns>
        /// <search>Data.DateTime.Now</search>
        [NodeCategory("Create")]
        public static System.DateTime Now(bool refresh = false)
        {
            return System.DateTime.Now;
        }

        /// <summary>
        /// Returns common formatted string representations of a datetime (or now).
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="standardFormat">Optional format to use.</param>
        /// <param name="dateFormat">Optional format to use.</param>
        /// <param name="timeFormat">Optional format to use.</param>
        /// <returns name="dateTime">The System.DateTime.</returns>
        /// <returns name="standard">Formatted string.</returns>
        /// <returns name="date">Formatted string.</returns>
        /// <returns name="time">Formatted string.</returns>
        /// <search>Data.DateTime.Formatted</search>
        [MultiReturn("dateTime", "standard", "date", "time")]
        [NodeCategory("Action")]
        public static Dictionary<string, object> Formatted([DefaultArgument("null")] System.DateTime? dateTime = null,
            string standardFormat = "dd/MM/yy HH:mm:ss",
            string dateFormat = "yyMMdd",
            string timeFormat = "HHmmss")
        {
            dateTime ??= System.DateTime.Now;

            return new Dictionary<string, object>()
            {
                { "dateTime", dateTime },
                { "standard", dateTime.Value.ToString(standardFormat, CultureInfo.InvariantCulture) },
                { "date", dateTime.Value.ToString(dateFormat, CultureInfo.InvariantCulture) },
                { "time", dateTime.Value.ToString(timeFormat, CultureInfo.InvariantCulture) }
            };
        }

        /// <summary>
        /// Returns a range of DateTimes across a period of time at specified intervals.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="startHour">Hour to start at.</param>
        /// <param name="endHour">Hour to end at.</param>
        /// <param name="timeStep">The number of steps to take per hour.</param>
        /// <param name="format">Optional format for prefix output.</param>
        /// <returns name="dateTimes">The System.DateTimes.</returns>
        /// <returns name="namePrefixes">Prefixes formatted to the datetimes.</returns>
        /// <search>Data.DateTime.AnalysisPeriod</search>
        [MultiReturn("dateTimes", "namePrefixes")]
        [NodeCategory("Create")]
        public static Dictionary<string, object> AnalysisPeriod(int year, int month, int day,
                int startHour, int endHour, int timeStep = 1, string format = "yyyy-MM-dd_HH-mm")
        {
            if (timeStep <= 0)
                throw new ArgumentException("Step must be greater than 0.");

            var dateTimes = new List<DateTime>();
            var names = new List<string>();

            int minuteStep = 60 / timeStep;

            for (int h = startHour; h <= endHour; h++)
            {
                for (int i = 0; i < timeStep; i++)
                {
                    int minute = i * minuteStep;

                    if (minute >= 60) continue;
                    if (h == endHour && minute > 0) continue;

                    var dt = new DateTime(year, month, day, h, minute, 0);

                    dateTimes.Add(dt);
                    names.Add(dt.ToString(format, CultureInfo.InvariantCulture));
                }
            }

            return new Dictionary<string, object>
            {
                { "dateTimes", dateTimes },
                { "namePrefixes", names }
            };
        }
    }
}