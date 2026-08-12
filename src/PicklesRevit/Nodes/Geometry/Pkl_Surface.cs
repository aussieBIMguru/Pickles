namespace Pkl_Geometry
{
    /// <summary>
    /// Nodes relating to Surfaces.
    /// </summary>
    public class Pkl_Surface
    {
        internal Pkl_Surface() { }

        /// <summary>
        /// Populates a set number of random points on a Surface.
        /// </summary>
        /// <param name="surface">Surface to populate.</param>
        /// <param name="count">Number of points.</param>
        /// <param name="seed">Randomization seed.</param>
        /// <returns name="points">The populated Points.</returns>
        /// <search>Geometry.Surface.Populate</search>
        [NodeCategory("Create")]
        public static List<DynPoint> Populate(DynSurface surface, int count = 1, int seed = 1)
        {
            List<DynPoint> points = new();
            if (surface is null || count < 1) { return points; }

            Random rand = new Random(seed);

            for (int i = 0; i < count; i++)
            {
                double u = rand.NextDouble();
                double v = rand.NextDouble();
                var point = surface.PointAtParameter(u, v);
                points.Add(point);
            }

            return points;
        }
    }
}