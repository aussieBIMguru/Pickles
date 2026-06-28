namespace Pickles.Extensions
{
    internal static class Ext_Int
    {
        internal static int Ext_Clamp(this int value, int max = 255, int min = 0)
        {
            return System.Math.Clamp(value, min, max);
        }
    }
}
