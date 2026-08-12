namespace Pkl_Geometry
{
    /// <summary>
    /// Nodes relating to Bounding Boxes.
    /// </summary>
    public class Pkl_BoundingBox
    {
        internal Pkl_BoundingBox() { }

        /// <summary>
        /// Creates a single BoundingBox around the provided ones.
        /// </summary>
        /// <param name="boundingBoxes">BoundingBoxes to combine.</param>
        /// <returns name="unionBox">The combined BoundingBox.</returns>
        /// <search>Geometry.BoundingBox.UnionBox</search>
        [NodeCategory("Create")]
        public static DynBb? UnionBox(List<DynBb> boundingBoxes)
        {
            if (boundingBoxes is null) { return null; }
            if (boundingBoxes.Count == 1) { return boundingBoxes[0]; }

            // Flatten all min/max points
            var minXs = boundingBoxes.Select(bb => bb.MinPoint.X);
            var minYs = boundingBoxes.Select(bb => bb.MinPoint.Y);
            var minZs = boundingBoxes.Select(bb => bb.MinPoint.Z);
            var maxXs = boundingBoxes.Select(bb => bb.MaxPoint.X);
            var maxYs = boundingBoxes.Select(bb => bb.MaxPoint.Y);
            var maxZs = boundingBoxes.Select(bb => bb.MaxPoint.Z);

            // Global extremes
            double minX = minXs.Min();
            double minY = minYs.Min();
            double minZ = minZs.Min();
            double maxX = maxXs.Max();
            double maxY = maxYs.Max();
            double maxZ = maxZs.Max();

            // Rebuild points, make new bounding box
            var minPoint = DynPoint.ByCoordinates(minX, minY, minZ);
            var maxPoint = DynPoint.ByCoordinates(maxX, maxY, maxZ);
            return DynBb.ByCorners(minPoint, maxPoint);
        }

        /// <summary>
        /// Gets the edge curves of the BoundingBox.
        /// </summary>
        /// <param name="boundingBox">BoundingBoxes to get edges from.</param>
        /// <returns name="curves">The edge curves.</returns>
        /// <search>Geometry.BoundingBox.GetEdgeCurves</search>
        [NodeCategory("Create")]
        public static IEnumerable<DynCurve> GetEdgeCurves(DynBb boundingBox)
        {
            if (boundingBox is null) { return new List<DynCurve>(); }

            var cuboid = boundingBox.ToCuboid();
            return cuboid.Edges.Select(c => c.CurveGeometry);
        }

        /// <summary>
        /// Returns the dimensions and centroid of the BoundingBox
        /// </summary>
        /// <param name="boundingBox">Bounding box to assess.</param>
        /// <returns name="width">Width (X) of the box.</returns>
        /// <returns name="depth">Depth (Y) of the box.</returns>
        /// <returns name="height">Height (Z) of the box.</returns>
        /// <returns name="centroid">3D centroid of the box.</returns>
        /// <search>Geometry.BoundingBox.Properties</search>
        [MultiReturn("width", "depth", "height", "centroid")]
        [NodeCategory("Query")]
        public static Dictionary<string, object> Properties(DynBb boundingBox)
        {
            var max = boundingBox.MinPoint;
            var min = boundingBox.MaxPoint;

            var centroid = DynPoint.ByCoordinates((min.X + max.X) / 2,
                (min.Y + max.Y) / 2,
                (min.Z + max.Z) / 2);

            return new Dictionary<string, object>()
            {
                { "width", (min.X +max.X) / 2 },
                { "depth", (min.Y +max.Y) / 2 },
                { "height", (min.Z +max.Z) / 2 },
                { "centroid", centroid }
            };
        }
    }
}