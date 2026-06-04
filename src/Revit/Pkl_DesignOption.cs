namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to DesignOptions.
    /// </summary>
    public class Pkl_DesignOption
    {
        internal Pkl_DesignOption() { }

        /// <summary>
        /// Gets the OptionSet related to a DesignOption.
        /// </summary>
        /// <param name="designOption">The DesignOption.</param>
        /// <returns name="optionSet">The DesignOptionSet.</returns>
        /// <search>design, option, optionset, set</search>
        public static DynElement? GetOptionSet(DynElement designOption)
        {
            DynElement optionSet = null;

            if (designOption?.InternalElement is DB.DesignOption option)
            {
                optionSet = option.Ext_GetDesignOptionSet().Ext_ToDynElement(true);
            }

            return optionSet;
        }
    }
}