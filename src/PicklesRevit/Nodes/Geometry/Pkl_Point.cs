namespace Pkl_Geometry
{
    /// <summary>
    /// Nodes relating to Points.
    /// </summary>
    public class Pkl_Point
    {
        internal Pkl_Point() { }

        /// <summary>
        /// Deconstructs a point into its coordinates.
        /// </summary>
        /// <param name="point">Point to deconstruct.</param>
        /// <returns name="x">X coordinate of point.</returns>
        /// <returns name="y">Y coordinate of point.</returns>
        /// <returns name="z">Z coordinate of point.</returns>
        /// <search>Geometry.Point.Deconstruct</search>
        [MultiReturn("x", "y", "z")]
        [NodeCategory("Query")]
        public static Dictionary<string, object> Deconstruct(DynPoint point)
        {
            return new Dictionary<string, object>()
            {
                { "x", point?.X },
                { "y", point?.Y },
                { "z", point?.Z }
            };
        }
    }
}