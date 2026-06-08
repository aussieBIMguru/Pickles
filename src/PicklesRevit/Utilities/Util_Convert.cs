namespace Pickles.Utilities
{
    /// <summary>
    /// Value conversion utility methods.
    /// </summary>
    internal static class Util_Convert
    {
        /// <summary>
        /// Mmmmm, Pi.
        /// </summary>
        internal static readonly double CONST_PI = Math.PI;

        /// <summary>
        /// Convert an angle.
        /// </summary>
        /// <param name="radians">The value to convert.</param>
        /// <returns>A double.</returns>
        internal static double RadiansToDegrees(double radians)
        {
            return radians * ((double)180 / CONST_PI);
        }

        /// <summary>
        /// Convert an angle.
        /// </summary>
        /// <param name="degrees">The value to convert.</param>
        /// <returns>A double.</returns>
        internal static double DegreesToRadians(double degrees)
        {
            return degrees * (CONST_PI / (double)180);
        }

        /// <summary>
        /// Convert a string to a nullable integer.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <param name="failValue">The value to return on failure to convert.</param>
        /// <returns>A nullable integer.</returns>
        public static int? StringToInt(string text, int? failValue = null)
        {
            int x = 0;

            if (Int32.TryParse(text, out x))
            {
                return x;
            }

            return failValue;
        }

        /// <summary>
        /// Convert a string to an integer.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <param name="failValue">The value to return on failure to convert.</param>
        /// <returns>An integer.</returns>
        public static int StringToIntoOrDefault(string text, int failValue = 0)
        {
            int x = 0;

            if (Int32.TryParse(text, out x))
            {
                return x;
            }

            return failValue;
        }

        /// <summary>
        /// Convert a string to a nullable double.
        /// </summary>
        /// <param name="text">The value to convert.</param>
        /// <param name="failValue">The value to return on failure to convert.</param>
        /// <returns>A nullable double.</returns>
        public static double? StringToDouble(string text, double? failValue = null)
        {
            double x = 0.0;

            if (double.TryParse(text, out x))
            {
                return x;
            }

            return failValue;
        }
    }
}
