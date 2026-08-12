namespace Pickles.Helpers
{
    /// <summary>
    /// Provides helper methods for calculating solar position (sun direction vector)
    /// based on geographic location, date, and time.
    /// </summary>
    public class LocationHelper
    {
        internal string Name { get; set; }
        internal double Latitude { get; set; }
        internal double Longitude { get; set; }
        internal double TimeZone { get; set; }
        internal double TimeZoneDst { get; set; }
        internal int DstBegins { get; set; }
        internal int DstEnds { get; set; }
        internal bool ObservesDst { get; set; }

        /// <summary>
        /// Initializes a location using explicit geographic coordinates and timezone.
        /// </summary>
        /// <param name="name">Name of the location.</param>
        /// <param name="latitude">Latitude in degrees.</param>
        /// <param name="longitude">Longitude in degrees.</param>
        /// <param name="timeZone">UTC offset in hours.</param>
        internal LocationHelper(string name, double latitude, double longitude, double timeZone)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            TimeZone = timeZone;
            TimeZoneDst = timeZone;
            ObservesDst = false;
        }

        /// <summary>
        /// Initializes a location from a Revit SiteLocation element.
        /// </summary>
        /// <param name="siteLocation">Revit site location containing geographic coordinates and timezone offset.</param>
        internal LocationHelper(DB.SiteLocation siteLocation)
        {
            Name = siteLocation.Name;
            Latitude = siteLocation.Latitude.Ext_ToDegrees();
            Longitude = siteLocation.Longitude.Ext_ToDegrees();
            TimeZone = siteLocation.TimeZone;
        }

        /// <summary>
        /// Defines daylight saving time rules for the location.
        /// </summary>
        /// <param name="offset"> DST offset in hours (typically +1.0 where applicable).</param>
        /// <param name="startMonth">Month DST begins (1–12).</param>
        /// <param name="endMonth">Month DST ends (1–12).</param>
        internal void SetDaylightSavings(double offset, int startMonth, int endMonth)
        {
            if (offset != 0.0)
                ObservesDst = true;

            TimeZoneDst = TimeZone + offset;

            DstBegins = startMonth;
            DstEnds = endMonth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        internal bool MonthIsInDaylightSavings(int month)
        {
            if (!ObservesDst) { return false; }

            if (DstBegins > DstEnds)
            {
                if (month >= DstBegins || month <= DstEnds)
                    return true;
            }
            // Standard northern hemisphere DST case
            else
            {
                if (month >= DstBegins && month <= DstEnds)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines the correct timezone offset for a given month,
        /// taking daylight saving rules into account if enabled.
        /// </summary>
        /// <param name="month">Month of the year (1–12).</param>
        /// <returns>UTC offset in hours.</returns>
        private double GetTimeZone(int month)
        {
            return MonthIsInDaylightSavings(month) ? TimeZoneDst : TimeZone;
        }

        /// <summary>
        /// Computes the sun direction vector for a specific DateTime.
        /// </summary>
        /// <param name="dateTime">Local date and time.</param>
        /// <returns>
        /// Normalized XYZ vector pointing toward the sun
        /// (X = East, Y = North, Z = Up).
        /// </returns>
        internal DB.XYZ GetSunVector(DateTime dateTime)
        {
            return GetSunVector(
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour + dateTime.Minute / 60.0,
                dateTime.Year
            );
        }

        /// <summary>
        /// Computes the sun direction vector using calendar date and time inputs.
        /// </summary>
        /// <param name="month">Month of year (1–12).</param>
        /// <param name="day">Day of month (1–31).</param>
        /// <param name="hour">Local time in decimal hours.</param>
        /// <param name="year">Optional year (defaults to current year if null).</param>
        /// <returns>
        /// Normalized XYZ vector pointing toward the sun
        /// (X = East, Y = North, Z = Up).
        /// </returns>
        internal DB.XYZ GetSunVector(int month, int day, double hour, int? year = null)
        {
            // Resolve year (defaults to current system year)
            int yearValue = year ?? DateTime.Now.Year;

            // Convert calendar date → day of year (handles leap years correctly)
            int dayOfYear = new DateTime(yearValue, month, day).DayOfYear;

            double lat = Latitude.Ext_ToRadians();

            // Solar declination: seasonal tilt of Earth relative to the sun
            double decl = 23.45.Ext_ToRadians() *
                          Math.Sin((360.0 / 365.0 * (dayOfYear - 81)).Ext_ToRadians());

            // Equation of Time: correction for Earth's orbital eccentricity
            double b = (360.0 / 365.0 * (dayOfYear - 81)).Ext_ToRadians();
            double eot =
                9.87 * Math.Sin(2 * b)
                - 7.53 * Math.Cos(b)
                - 1.5 * Math.Sin(b);

            // Solar time correction (longitude + timezone offset)
            double lstm = 15.0 * GetTimeZone(month); // local standard meridian
            double timeCorrection = 4.0 * (Longitude - lstm) + eot;
            double solarTime = hour + timeCorrection / 60.0;

            // Hour angle: angular displacement of the sun from solar noon
            double hourAngle = (15.0 * (solarTime - 12.0)).Ext_ToRadians();

            // Solar altitude: elevation angle above horizon
            double sinAlt =
                Math.Sin(lat) * Math.Sin(decl) +
                Math.Cos(lat) * Math.Cos(decl) * Math.Cos(hourAngle);

            sinAlt = Math.Max(-1.0, Math.Min(1.0, sinAlt));

            double altitude = Math.Asin(sinAlt);
            double cosAlt = Math.Cos(altitude);

            // Edge case: sun directly overhead (azimuth undefined)
            if (Math.Abs(cosAlt) < 1e-8)
                return new DB.XYZ(0, 0, Math.Sign(Math.Sin(altitude)));

            // Solar azimuth: compass direction of the sun
            double sinAz = -Math.Sin(hourAngle) * Math.Cos(decl) / cosAlt;

            double cosAz =
                (Math.Sin(decl) - Math.Sin(lat) * sinAlt) /
                (Math.Cos(lat) * cosAlt);

            double azimuth = Math.Atan2(sinAz, cosAz);

            if (azimuth < 0)
                azimuth += 2.0 * Math.PI;

            // Convert spherical coordinates → Cartesian vector
            double x = Math.Sin(azimuth) * cosAlt; // East
            double y = Math.Cos(azimuth) * cosAlt; // North
            double z = Math.Sin(altitude);         // Up

            return new DB.XYZ(x, y, z).Normalize();
        }

        /// <summary>
        /// Computes the sun direction vector and converts it into Revit project coordinates
        /// using the document's True North rotation.
        /// </summary>
        /// <param name="month">Month of year (1–12).</param>
        /// <param name="day">Day of month (1–31).</param>
        /// <param name="hour">Local time in decimal hours.</param>
        /// <param name="year">Optional year (defaults to current year if null).</param>
        /// <param name="doc">Revit document used to extract project True North rotation.
        /// If null, raw geographic sun vector is returned.</param>
        /// <returns>Normalized XYZ vector representing sun direction in Revit coordinates.
        /// X = East, Y = North, Z = Up.</returns>
        internal DB.XYZ GetRevitSunVector(int month, int day, double hour, DB.Document doc, int? year = null)
        {
            // Compute sun vector in geographic / true north coordinate system
            DB.XYZ sun = GetSunVector(month, day, hour, year);

            // If no Revit document is available, return unmodified vector
            if (doc is null)
                return sun;

            // Extract True North rotation from the active project location
            double trueNorthRadians =
                doc.ActiveProjectLocation
                   .GetProjectPosition(DB.XYZ.Zero)
                   .Angle;

            // Convert from True North space → Revit Project North space
            DB.Transform rotation = DB.Transform.CreateRotation(
                DB.XYZ.BasisZ,
                -trueNorthRadians
            );

            // Apply coordinate transformation
            return rotation.OfVector(sun);
        }

        /// <summary>
        /// Computes the sun direction vector and converts it into Revit project coordinates
        /// using the document's True North rotation.
        /// </summary>
        /// <param name="dateTime">Local date and time used for solar position calculation.</param>
        /// <param name="doc">Revit document used to extract project True North rotation.
        /// If null, raw geographic sun vector is returned.</param>
        /// <returns>
        /// Normalized XYZ vector representing sun direction in Revit coordinates.
        /// X = East, Y = North, Z = Up.
        /// </returns>
        internal DB.XYZ GetRevitSunVector(DateTime dateTime, DB.Document doc)
        {
            return GetRevitSunVector(
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour + dateTime.Minute / 60.0,
                doc,
                dateTime.Year
            );
        }
    }
}