namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to Pickle specific types.
    /// </summary>
    public class Pkl_UnitInfo
    {
        internal Pkl_UnitInfo() { }

        /// <summary>
        /// Gets the underlying ForgeTypeId the object holds.
        /// </summary>
        /// <param name="unitInfo">The unit information.</param>
        /// <returns name="forgeTypeId">The ForgeTypeId.</returns>
        /// <search>Data.UnitInfo.ForgeTypeId</search>
        [NodeCategory("Query")]
        public static DB.ForgeTypeId? ForgeTypeId(object unitInfo)
        {
            if (unitInfo is Pickles.UnitInfo pklObj)
            {
                return pklObj.ForgeTypeId;
            }

            throw new ArgumentException("Input must be a Pickles.UnitInfo");
        }

        /// <summary>
        /// Gets the underlying name that the object holds.
        /// </summary>
        /// <param name="unitInfo">The unit information.</param>
        /// <returns name="name">The unit name.</returns>
        /// <search>Data.UnitInfo.Name</search>
        [NodeCategory("Query")]
        public static string Name(object unitInfo)
        {
            if (unitInfo is Pickles.UnitInfo pklObj)
            {
                return pklObj.UnitName;
            }

            throw new ArgumentException("Input must be a Pickles.UnitInfo");
        }

        /// <summary>
        /// Gets the underlying TypeId that the object holds.
        /// </summary>
        /// <param name="unitInfo">The unit information.</param>
        /// <returns name="name">The unit TypeId.</returns>
        /// <search>Data.UnitInfo.TypeId</search>
        [NodeCategory("Query")]
        public static string TypeId(object unitInfo)
        {
            if (unitInfo is Pickles.UnitInfo pklObj)
            {
                return pklObj.TypeId;
            }

            throw new ArgumentException("Input must be a Pickles.UnitInfo");
        }
    }
}

namespace Pickles
{
    /// <summary>
    /// Wrapper to hold/pass UnitType information.
    /// </summary>
    internal class UnitInfo
    {
        internal string TypeId { get; }
        internal DB.ForgeTypeId ForgeTypeId { get; }
        internal string UnitName => DB.LabelUtils.GetLabelForUnit(ForgeTypeId);

        internal UnitInfo(string typeId)
        {
            TypeId = typeId;
            ForgeTypeId = new DB.ForgeTypeId(typeId);
        }

        internal UnitInfo(DB.ForgeTypeId forgeTypeId)
        {
            TypeId = forgeTypeId.TypeId;
            ForgeTypeId = forgeTypeId;
        }
    }
}