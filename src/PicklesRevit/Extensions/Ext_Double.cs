using System;
using System.Collections.Generic;
using System.Text;

namespace Pickles.Extensions
{
    internal static class Ext_Double
    {
        /// <summary>
        /// Convert an angle.
        /// </summary>
        /// <param name="radians">The value to convert.</param>
        /// <returns>A double.</returns>
        internal static double Ext_ToDegrees(this double radians)
        {
            return radians * ((double)180 / Math.PI);
        }

        /// <summary>
        /// Convert an angle.
        /// </summary>
        /// <param name="degrees">The value to convert.</param>
        /// <returns>A double.</returns>
        internal static double Ext_ToRadians(this double degrees)
        {
            return degrees * (Math.PI / (double)180);
        }
    }
}
