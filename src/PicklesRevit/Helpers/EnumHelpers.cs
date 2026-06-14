namespace Pickles.Helpers
{
    internal class EnumHelpers
    {
        internal static T EnumByName<T>(string name, T fallbackValue) where T : struct, Enum
        {
            if (Enum.TryParse(name, out T t))
            {
                return t;
            }
            else
            {
                return fallbackValue;
            }
        }
    }
}
