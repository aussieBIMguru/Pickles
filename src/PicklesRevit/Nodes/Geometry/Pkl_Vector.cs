namespace Pkl_Geometry
{
    /// <summary>
    /// Nodes relating to Vectors.
    /// </summary>
    public class Pkl_Vector
    {
        internal Pkl_Vector() { }

        /// <summary>
        /// Deconstructs a Vector into its coordinates.
        /// </summary>
        /// <param name="vector">Vector to deconstruct.</param>
        /// <returns name="x">X coordinate of vector.</returns>
        /// <returns name="y">Y coordinate of vector.</returns>
        /// <returns name="z">Z coordinate of vector.</returns>
        /// <search>Geometry.Vector.Deconstruct</search>
        [MultiReturn("x", "y", "z")]
        [NodeCategory("Query")]
        public static Dictionary<string, object> Deconstruct(DynVector vector)
        {
            return new Dictionary<string, object>()
            {
                { "x", vector?.X },
                { "y", vector?.Y },
                { "z", vector?.Z }
            };
        }
    }
}