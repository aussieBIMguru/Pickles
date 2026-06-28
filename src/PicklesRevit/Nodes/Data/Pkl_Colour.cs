namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to colours.
    /// </summary>
    public class Pkl_Colour
    {
        internal Pkl_Colour() { }

        /// <summary>
        /// Overrides the values of a colour.
        /// </summary>
        /// <param name="colour">The colour to override.</param>
        /// <param name="a">Optional alpha override value.</param>
        /// <param name="r">Optional red override value.</param>
        /// <param name="g">Optional green override value.</param>
        /// <param name="b">Optional blue override value.</param>
        /// <returns name="colour">The updated colour.</returns>
        /// <search>Data.Colour.OverrideValues</search>
        [NodeCategory("Action")]
        public static DSCore.Color OverrideValues(DSCore.Color colour,
            [DefaultArgument("null")] int? a = null,
            [DefaultArgument("null")] int? r = null,
            [DefaultArgument("null")] int? g = null,
            [DefaultArgument("null")] int? b = null)
        {
            int ax = (a ?? colour.Alpha).Ext_Clamp(255, 0);
            int rx = (r ?? colour.Red).Ext_Clamp(255, 0);
            int gx = (g ?? colour.Green).Ext_Clamp(255, 0);
            int bx = (b ?? colour.Blue).Ext_Clamp(255, 0);

            return DSCore.Color.ByARGB(ax, rx, gx, bx);
        }

        /// <summary>
        /// Desaturates a colour to grayscale.
        /// </summary>
        /// <param name="colour">The colour to desaturate</param>
        /// <returns name="colour">The updated colour.</returns>
        /// <search>Data.Colour.Desaturate</search>
        [NodeCategory("Action")]
        public static DSCore.Color Desaturate(DSCore.Color colour)
        {
            int gray = (colour.Red + colour.Green + colour.Blue) / 3;
            return DSCore.Color.ByARGB(colour.Alpha, gray, gray, gray);
        }
    }
}