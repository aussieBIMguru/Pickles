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