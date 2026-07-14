using Autodesk.Revit.DB;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to DesignOptions.
    /// </summary>
    public class Pkl_DesignOption
    {
        internal Pkl_DesignOption() { }

        /// <summary>
        /// Gets the Elements related to a DesignOption.
        /// </summary>
        /// <param name="designOption">The DesignOption.</param>
        /// <returns name="elements">The related Elements.</returns>
        /// <search>Revit.DesignOption.GetElements</search>
        [NodeCategory("Action")]
        public static IList<DynElement> GetElements(DynElement designOption)
        {
            return designOption.InternalElement.Document.Ext_Collector()
                .WherePasses(new DB.ElementDesignOptionFilter(designOption.InternalElement.Id))
                .Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Gets the OptionSet related to a DesignOption.
        /// </summary>
        /// <param name="designOption">The DesignOption.</param>
        /// <returns name="optionSet">The DesignOptionSet.</returns>
        /// <search>Revit.DesignOption.GetOptionSet</search>
        [NodeCategory("Query")]
        public static DynElement? GetOptionSet(DynElement designOption)
        {
            DynElement? optionSet = null;

            if (designOption?.InternalElement is DB.DesignOption option)
            {
                optionSet = option.Ext_GetDesignOptionSet()?.Ext_ToDynElement(true);
            }

            return optionSet;
        }

        /// <summary>
        /// Returns if a DesignOption is a primary option in its set.
        /// </summary>
        /// <param name="designOption">The DesignOption.</param>
        /// <returns name="isPrimary">Is the DesignOption primary.</returns>
        /// <search>Revit.DesignOption.IsPrimary</search>
        [NodeCategory("Query")]
        public static bool IsPrimary(DynElement designOption)
        {
            if (designOption?.InternalElement is DB.DesignOption option)
            {
                return option.IsPrimary;
            }

            return false;
        }
    }
}