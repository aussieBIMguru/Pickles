namespace Pickle.Extensions
{
    /// <summary>
    /// Extension methods for DB DesignOptions.
    /// </summary>
    internal static class Ext_DBDesignOption
    {
        /// <summary>
        /// Gets the DesignOptionSet of a DesignOption.
        /// </summary>
        /// <param name="designOption">The DesignOption.</param>
        /// <returns>A DesignOptionSet Element.</returns>
        internal static DB.Element? Ext_GetDesignOptionSet(this DB.DesignOption designOption)
        {
            // Null guard
            if (designOption is null) { return null; }

            // Get the option set Id
            DB.Parameter parameter = designOption.Ext_GetBuiltInParameter(DB.BuiltInParameter.OPTION_SET_ID);
            DB.ElementId setId = parameter?.AsElementId();

            // Return the DesignOptionSet (Element)
            return setId.Ext_GetElement<DB.Element>(designOption.Document);
        }
    }
}
