namespace Pkl_Geometry
{
    /// <summary>
    /// Nodes relating to modifying Geometry.
    /// </summary>
    public class Pkl_Modifiers
    {
        internal Pkl_Modifiers() { }

        /// <summary>
        /// Splits a list of geometry by Solid and non-Solid objects.
        /// </summary>
        /// <param name="geometries">Geometry to filter.</param>
        /// <returns name="solids">Solid objects.</returns>
        /// <returns name="nonSolids">Non-Solid objects.</returns>
        /// <search>Geometry.Modifiers.FilterSolids</search>
        [MultiReturn("solids", "nonSolids")]
        [NodeCategory("Create")]
        public static Dictionary<string, object> FilterSolids(List<DynGeometry> geometries)
        {
            // Construct outputs
            List<DynSolid> solids = new();
            List<DynGeometry> nonSolids = new();

            var output = new Dictionary<string, object>()
            {
                { "solids", solids },
                { "nonSolids", nonSolids }
            };

            // Filter the geometry by being a solid
            foreach (DynGeometry geometry in geometries)
            {
                if (geometry is DynSolid solid)
                {
                    solids.Add(solid);
                }
                else
                {
                    nonSolids.Add(geometry);
                }
            }

            // Return output
            return output;
        }
    }
}