namespace PicklesUI.Extensions
{
    internal static class String_Ext
    {
        internal static string Ext_ExtractPickleKey(this string pickle)
        {
            if (string.IsNullOrEmpty(pickle))
                return pickle;

            string[] parts = pickle.Split(
                new[] { "§§§" },
                3,
                StringSplitOptions.None);

            return parts.Length == 3
                ? parts[2]
                : pickle;
        }
    }
}
