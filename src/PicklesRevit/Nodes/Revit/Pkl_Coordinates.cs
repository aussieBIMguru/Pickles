using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Coordinates.
    /// </summary>
    public class Pkl_Coordinates
    {
        internal Pkl_Coordinates() { }

        /// <summary>
        /// Creates a point at the specified real world coordinate values.
        /// </summary>
        /// <param name="easting">The easting (X) coordinate value.</param>
        /// <param name="northing">The northing (Y) coordinate value.</param>
        /// <param name="elevation">The elevation (Z) coordinate value.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="point">The Dynamo point.</returns>
        /// <search>Revit.Coordinates.GetPointAtRealWorldValues</search>
        [NodeCategory("Create")]
        public static DynPoint? GetPointAtRealWorldValues(double easting, double northing,
            double elevation, [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);
            DB.Document doc = docHelper.Document;

            // Get the project position at the internal origin
            DB.ProjectPosition origin =
                doc.ActiveProjectLocation.GetProjectPosition(DB.XYZ.Zero);

            // Remove the project offsets
            double x = easting - origin.EastWest;
            double y = northing - origin.NorthSouth;
            double z = elevation - origin.Elevation;

            // Rotate back into internal coordinates
            double angle = origin.Angle;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            double internalX = x * cos + y * sin;
            double internalY = -x * sin + y * cos;

            return new DB.XYZ(internalX, internalY, z).ToPoint();
        }

        /// <summary>
        /// Gets the real world coordinate values of a point in Revit.
        /// </summary>
        /// <param name="point">The point in Dynamo.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="easting">The easting (X) coordinate value.</returns>
        /// <returns name="northing">The northing (Y) coordinate value.</returns>
        /// <returns name="elevation">The elevation (Z) coordinate value.</returns>
        /// <search>Revit.Coordinates.GetRealWorldValuesAtPoint</search>
        [NodeCategory("Action")]
        [MultiReturn("easting", "northing", "elevation")]
        public static Dictionary<string, double> GetRealWorldValuesAtPoint(DynPoint point,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);
            DB.Document doc = docHelper.Document;

            // Get the project position of the point
            DB.ProjectPosition position =
                doc.ActiveProjectLocation.GetProjectPosition(point.ToRevitType());

            return new Dictionary<string, double>
            {
                { "easting", position.EastWest },
                { "northing", position.NorthSouth },
                { "elevation", position.Elevation }
            };
        }

        /// <summary>
        /// Returns the internal origin and its coordinate values.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="easting">The easting (X) coordinate value.</returns>
        /// <returns name="northing">The northing (Y) coordinate value.</returns>
        /// <returns name="elevation">The elevation (Z) coordinate value.</returns>
        /// <search>Revit.Coordinates.InternalOrigin</search>
        [NodeCategory("Query")]
        [MultiReturn("point", "easting", "northing", "elevation")]
        public static Dictionary<string, object> InternalOrigin(
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get related document
            var docHelper = new DocumentHelper(docOrLinkInstance, true);
            DB.Document doc = docHelper.Document;

            // Get project origin and units
            DB.ProjectPosition position = doc.ActiveProjectLocation.GetProjectPosition(DB.XYZ.Zero);
            DB.ForgeTypeId unitId = doc.GetUnits().GetFormatOptions(DB.SpecTypeId.Length).GetUnitTypeId();

            // Return point and coordinates
            return new Dictionary<string, object>
            {
                { "point", DB.XYZ.Zero.ToPoint() },
                { "easting", DB.UnitUtils.ConvertFromInternalUnits(position.EastWest, unitId) },
                { "northing", DB.UnitUtils.ConvertFromInternalUnits(position.NorthSouth, unitId) },
                { "elevation", DB.UnitUtils.ConvertFromInternalUnits(position.Elevation, unitId) }
            };
        }

        /// <summary>
        /// Returns the project base point and its coordinate values.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="point">The Project Base Point location.</returns>
        /// <returns name="easting">The easting (X) coordinate value.</returns>
        /// <returns name="northing">The northing (Y) coordinate value.</returns>
        /// <returns name="elevation">The elevation (Z) coordinate value.</returns>
        /// <returns name="angle">The angle to true north in degrees.</returns>
        /// <search>Revit.Coordinates.ProjectBasePoint</search>
        [NodeCategory("Query")]
        [MultiReturn("point", "easting", "northing", "elevation", "angle")]
        public static Dictionary<string, object> ProjectBasePoint(
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get related document
            var docHelper = new DocumentHelper(docOrLinkInstance, true);
            DB.Document doc = docHelper.Document;

            // Get project base point and project units
            DB.BasePoint basePoint = DB.BasePoint.GetProjectBasePoint(doc);
            DB.ForgeTypeId unitId = doc.GetUnits().GetFormatOptions(DB.SpecTypeId.Length).GetUnitTypeId();

            // Return point and coordinates
            return new Dictionary<string, object>
            {
                { "point", basePoint.Position.ToPoint() },
                { "easting", basePoint.Ext_GetBuiltInParameter(DB.BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble().Ext_InternalToProject(unitId) },
                { "northing", basePoint.Ext_GetBuiltInParameter(DB.BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble().Ext_InternalToProject(unitId) },
                { "elevation", basePoint.Ext_GetBuiltInParameter(DB.BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble().Ext_InternalToProject(unitId) },
                { "angle", basePoint.Ext_GetBuiltInParameter(DB.BuiltInParameter.BASEPOINT_ANGLETON_PARAM).AsDouble().Ext_ToDegrees() }
            };
        }

        /// <summary>
        /// Returns the survey point and its coordinate values.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="point">The Survey Point location.</returns>
        /// <returns name="easting">The easting (X) coordinate value.</returns>
        /// <returns name="northing">The northing (Y) coordinate value.</returns>
        /// <returns name="elevation">The elevation (Z) coordinate value.</returns>
        /// <search>Revit.Coordinates.SurveyPoint</search>
        [NodeCategory("Query")]
        [MultiReturn("point", "easting", "northing", "elevation")]
        public static Dictionary<string, object> SurveyPoint(
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get related document
            var docHelper = new DocumentHelper(docOrLinkInstance, true);
            DB.Document doc = docHelper.Document;

            // Get project base point and project units
            DB.BasePoint surveyPoint = DB.BasePoint.GetSurveyPoint(doc);
            DB.ForgeTypeId unitId = doc.GetUnits().GetFormatOptions(DB.SpecTypeId.Length).GetUnitTypeId();

            // Return point and coordinates
            return new Dictionary<string, object>
            {
                { "point", surveyPoint.Position.ToPoint() },
                { "easting", surveyPoint.Ext_GetBuiltInParameter(DB.BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble().Ext_InternalToProject(unitId) },
                { "northing", surveyPoint.Ext_GetBuiltInParameter(DB.BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble().Ext_InternalToProject(unitId) },
                { "elevation", surveyPoint.Ext_GetBuiltInParameter(DB.BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble().Ext_InternalToProject(unitId) },
            };
        }
    }
}