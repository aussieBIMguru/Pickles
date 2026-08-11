using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Areas.
    /// </summary>
    public class Pkl_Area
    {
        internal Pkl_Area() { }

        /// <summary>
        /// Creates Areas at the supplied points in the specified Views.
        /// </summary>
        /// <param name="points">The points at which to create Areas.</param>
        /// <param name="views">The Views in which to create the Areas.</param>
        /// <returns name="areas">The created Areas.</returns>
        /// <search>Revit.Area.Create</search>
        [NodeCategory("Create")]
        public static List<DynElement?> CreateAreas(List<DynPoint> points, List<DynView> views)
        {
            if (points == null || points.Count == 0)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return [];
            }

            // Match Views to Points
            IList<DB.ViewPlan> dbViews = views
                .Select(v => v.InternalElement)
                .OfType<DB.ViewPlan>()
                .ToList();

            if (dbViews == null || dbViews.Count == 0)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return Enumerable.Repeat<DynElement?>(null, points.Count).ToList();
            }

            // Match Views to Points
            dbViews = dbViews.Ext_LaceLongest(points.Count);

            DB.Document doc = views[0].InternalElement.Document;

            // Create Areas
            doc.Ext_EnsureTransaction();

            List<DynElement?> areas = [];

            for (int i = 0; i < points.Count; i++)
            {
                try
                {
                    DB.XYZ xyz = points[i].ToXyz();
                    DB.UV uv = new(xyz.X, xyz.Y);
                    DB.Area area = doc.Create.NewArea(dbViews[i], uv);
                    areas.Add(area.Ext_ToDynElement(true));
                }
                catch
                {
                    areas.Add(null);
                }
            }

            doc.Ext_TransactionDone();

            return areas;
        }

        /// <summary>
        /// Creates Areas from Rooms in the supplied Views.
        /// </summary>
        /// <param name="rooms">The Rooms from which to create the Areas.</param>
        /// <param name="views">The Views in which to create the Areas.</param>
        /// <returns name="areas">The created Areas.</returns>
        /// <search>Revit.Area.CreateFromRooms</search>
        [NodeCategory("Create")]
        public static List<DynElement?> CreateAreasFromRooms(List<DynElement> rooms,
            List<DynView> views)
        {
            if (rooms == null || rooms.Count == 0)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return [];
            }

            // Match Views to Rooms
            IList<DB.ViewPlan> dbViews = views
                .Select(v => v.InternalElement)
                .OfType<DB.ViewPlan>()
                .ToList()
                .Ext_LaceLongest(rooms.Count);

            if (dbViews.Count == 0)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return Enumerable.Repeat<DynElement?>(null, rooms.Count).ToList();
            }

            DB.Document doc = views[0].InternalElement.Document;

            // Create Areas
            doc.Ext_EnsureTransaction();

            List<DynElement?> areas = [];

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].InternalElement is not DB.Architecture.Room room
                    || room.Location is not DB.LocationPoint location)
                {
                    areas.Add(null);
                    continue;
                }

                try
                {
                    DB.XYZ point = location.Point;
                    DB.UV uv = new(point.X, point.Y);
                    DB.Area area = doc.Create.NewArea(dbViews[i], uv);
                    area.Number = room.Number;
                    area.Name = room.Name;
                    areas.Add(area.Ext_ToDynElement(true));
                }
                catch
                {
                    areas.Add(null);
                }
            }

            doc.Ext_TransactionDone();

            return areas;
        }

        /// <summary>
        /// Creates Area Boundary Lines from curves in the supplied Views.
        /// </summary>
        /// <param name="curves">The curves from which to create the boundary lines.</param>
        /// <param name="views">The Views in which to create the boundary lines.</param>
        /// <returns name="boundaries">The created Area Boundary Lines.</returns>
        /// <search>Revit.AreaBoundaryLine.Create</search>
        [NodeCategory("Create")]
        public static List<DynElement?> CreateAreaBoundaryLines(List<DynCurve> curves,
            List<DynView> views)
        {
            if (curves == null || curves.Count == 0)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return [];
            }

            // Match Views to Curves
            IList<DB.ViewPlan> dbViews = views
                .Select(v => v.InternalElement)
                .OfType<DB.ViewPlan>()
                .ToList()
                .Ext_LaceLongest(curves.Count);

            if (dbViews.Count == 0)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return Enumerable.Repeat<DynElement?>(null, curves.Count).ToList();
            }

            DB.Document doc = views[0].InternalElement.Document;

            // Create Area Boundary Lines
            doc.Ext_EnsureTransaction();

            List<DynElement?> boundaries = [];

            for (int i = 0; i < curves.Count; i++)
            {
                try
                {
                    DB.ViewPlan view = dbViews[i];
                    DB.Level? level = view.GenLevel;

                    if (level == null)
                    {
                        boundaries.Add(null);
                        continue;
                    }

                    var sketchPlane = DB.SketchPlane.Create(doc, level.Id);
                    DB.Curve curve = curves[i].ToRevitType();

                    DB.ModelCurve boundary =
                        doc.Create.NewAreaBoundaryLine(
                            sketchPlane,
                            curve,
                            view);

                    boundaries.Add(boundary.Ext_ToDynElement(true));
                }
                catch
                {
                    boundaries.Add(null);
                }
            }

            doc.Ext_TransactionDone();

            return boundaries;
        }
    }
}
