namespace Pkl_Geometry
{
    /// <summary>
    /// Nodes relating to PolyCurves.
    /// </summary>
    public class Pkl_PolyCurve
    {
        internal Pkl_PolyCurve() { }

        /// <summary>
        /// Filters the largest closed PolyCurve from a list, and returns all other closed Surfaces as a separate output.
        /// </summary>
        /// <param name="polyCurves">Polycurves to process.</param>
        /// <returns name="greatestArea">The closed PolyCurve with the largest area.</returns>
        /// <returns name="theRest">The other closed PolyCurves as a separate output.</returns>
        /// <search>Geometry.PolyCurve.FindGreatestArea</search>
        [MultiReturn("greatestArea", "theRest")]
        [NodeCategory("Action")]
        public static Dictionary<string, object> FindGreatestArea(List<DynPolyCurve> polyCurves)
        {
            var result = FilterPolyCurves(polyCurves);
            
            return new Dictionary<string, object>()
            {
                { "greatestArea", result.GreatestArea },
                { "theRest", result.TheRest }
            };
        }

        /// <summary>
        /// Attempts to cut the internalized PolyCurves from the largest one.
        /// This node assumes all nodes are coplanar and at least one PolyCurve envelops the others.
        /// </summary>
        /// <param name="polyCurves">Polycurves to process.</param>
        /// <returns name="surface">The overall surface difference.</returns>
        /// <search>Geometry.PolyCurve.Swissify</search>
        [NodeCategory("Create")]
        public static DynSurface? Swissify(List<DynPolyCurve> polyCurves)
        {
            var result = FilterPolyCurves(polyCurves);

            DynSurface cheese = result.GreatestArea.Patch();
            if (result.GreatestArea == null) { return null; }
            List<DynSurface> holes = result.TheRest.Select(h => h.Patch()).ToList();

            DynSurface swissSurface = cheese.Difference(holes);

            cheese.Dispose();
            holes.ForEach(h => h.Dispose());

            return swissSurface;
        }

        /// <summary>
        /// Helper method to get largest area polycurve and remainder as a tuple.
        /// </summary>
        /// <param name="polyCurves"></param>
        /// <returns></returns>
        private static (DynPolyCurve GreatestArea, List<DynPolyCurve> TheRest)
            FilterPolyCurves(List<DynPolyCurve> polyCurves)
        {
            var sorted = polyCurves
                .Where(pc => pc?.IsClosed == true)
                .Select(pc =>
                {
                    using DynSurface surface = pc.Patch();
                    return (Curve: pc, Area: surface.Area);
                })
                .OrderByDescending(x => x.Area)
                .Select(x => x.Curve)
                .ToList();

            if (sorted.Count == 0)
            {
                return (null, new List<DynPolyCurve>());
            }

            var outerLoop = sorted[0];
            sorted.RemoveAt(0);

            return (outerLoop, sorted);
        }
    }
}