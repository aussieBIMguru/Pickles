namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to LocationHelpers.
    /// </summary>
    public class Pkl_Location
    {
        internal Pkl_Location() { }

        /// <summary>
        /// Constructs a LocationHelper object from values.
        /// </summary>
        /// <param name="name">The Location name.</param>
        /// <param name="latitude">The Latitude value.</param>
        /// <param name="longitude">The Longitude value.</param>
        /// <param name="timezone">The standard timezone GMT offset.</param>
        /// <returns name="locationHelper">A Pickles.LocationHelper object.</returns>
        /// <search>Data.Location.Construct</search>
        [NodeCategory("Create")]
        public static LocationHelper Construct(string name, double latitude, double longitude,
            double timezone)
        {
            return new LocationHelper(
                name: name,
                latitude: latitude,
                longitude: longitude,
                timeZone: timezone);
        }

        /// <summary>
        /// Deconstructs a LocationHelper into its values.
        /// </summary>
        /// <param name="locationHelper">The LocationHelper to deconstruct.</param>
        /// <returns name="name">The Location name.</returns>
        /// <returns name="latitude">The Latitude value.</returns>
        /// <returns name="longitude">The Longitude value.</returns>
        /// <returns name="timeZone">The standard timezone GMT offset.</returns>
        /// <search>Data.Location.Deconstruct</search>
        [NodeCategory("Action")]
        public static Dictionary<string, object> Deconstruct(LocationHelper locationHelper)
        {
            return new Dictionary<string, object>()
            {
                { "name", locationHelper.Name },
                { "latitude", locationHelper.Latitude },
                { "longitude", locationHelper.Longitude },
                { "timeZone", locationHelper.TimeZone }
            };
        }

        /// <summary>
        /// Introduces Daylight Savings to a LocationHelper.
        /// </summary>
        /// <param name="locationHelper">The LocationHelper to modify.</param>
        /// <param name="offset">The timezone difference.</param>
        /// <param name="startMonth">The first month daylight savings applies in.</param>
        /// <param name="endMonth">The last month daylight savings applies in.</param>
        /// <returns name="locationHelper">The modified LocationHelper object.</returns>
        /// <search>Data.Location.AddDaylightSavings</search>
        [NodeCategory("Action")]
        public static LocationHelper AddDaylightSavings(LocationHelper locationHelper,
            double offset, int startMonth, int endMonth)
        {
            locationHelper.SetDaylightSavings(offset, startMonth, endMonth);
            return locationHelper;
        }

        /// <summary>
        /// Returns if a date falls within daylight savings for a location.
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <param name="locationHelper">The LocationHelper to use.</param>
        /// <returns name="locationHelper">The modified LocationHelper object.</returns>
        /// <search>Data.Location.IsDaylightSavings</search>
        [NodeCategory("Query")]
        public static bool IsDaylightSavings(DateTime dateTime, LocationHelper locationHelper)
        {
            return locationHelper.MonthIsInDaylightSavings(dateTime.Month);
        }

        /// <summary>
        /// Returns the sun direction at a specified DateTime (with true north as the internal Y axis).
        /// </summary>
        /// <param name="dateTime">The DateTime to check.</param>
        /// <param name="locationHelper">The LocationHelper to use.</param>
        /// <returns name="vector">The direction of the sun.</returns>
        /// <search>Data.Location.GetInternalSunDirection</search>
        [NodeCategory("Action")]
        public static DynVector GetInternalSunDirection(DateTime dateTime, LocationHelper locationHelper)
        {
            return locationHelper
                .GetSunVector(dateTime)
                .Ext_ToDynamoVector();
        }

        /// <summary>
        /// Returns the sun direction in Revit true north–aligned coordinates.
        /// </summary>
        /// <param name="dateTime">The DateTime to get the direction of.</param>
        /// <param name="locationHelper">The LocationHelper to use.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="vector">The direction of the sun.</returns>
        /// <search>Data.Location.GetRevitSunDirection</search>
        [NodeCategory("Action")]
        public static DynVector? GetRevitSunDirection(DateTime dateTime, LocationHelper locationHelper,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return null;
            }

            // Return the sun direction
            return locationHelper
                .GetRevitSunVector(dateTime, docHelper.Document)
                .Ext_ToDynamoVector();
        }
    }
}