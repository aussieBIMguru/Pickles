using System;
using System.Collections.Generic;
using System.Text;

namespace Pickles.Extensions
{
    internal static class Ext_DynElements
    {
        internal static DB.ForgeTypeId Ext_ToSpecTypeId(this DynSpecType specType)
        {
            return new DB.ForgeTypeId(specType.TypeId);
        }

        internal static DB.ForgeTypeId Ext_ToGroupTypeId(this DynGroupType groupType)
        {
            return new DB.ForgeTypeId(groupType.TypeId);
        }
    }
}
