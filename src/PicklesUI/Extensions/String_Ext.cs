using Autodesk.Revit.DB;

namespace PicklesUI.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class String_Ext
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

        internal static ForgeTypeId Ext_ToForgeTypeId(this string id)
        {
            return new ForgeTypeId(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ForgeTypeId StringToForgeTypeId(string id)
        {
            return new ForgeTypeId(id);
        }
    }
}
