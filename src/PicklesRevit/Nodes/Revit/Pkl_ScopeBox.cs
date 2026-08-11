namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to ScopeBoxes.
    /// </summary>
    public class Pkl_ScopeBox
    {
        internal Pkl_ScopeBox() { }

        private static IList<DynElement?> ScopedElements<T>(
            DynElement scopeBox,
            DB.BuiltInParameter parameter) where T : DB.Element
        {
            if (scopeBox?.InternalElement is not DB.Element dbScopeBox)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return [];
            }

            return dbScopeBox.Document
                .Ext_CollectByClass<T>()
                .Where(e => e.Ext_GetBuiltInParameter(parameter)
                    ?.AsElementId() == dbScopeBox.Id)
                .Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Gets all Views that use the supplied Scope Box.
        /// </summary>
        /// <param name="scopeBox">The Scope Box.</param>
        /// <returns name="views">Views using the Scope Box.</returns>
        /// <search>Revit.ScopeBox.Views</search>
        [NodeCategory("Query")]
        public static IList<DynElement?> Views(DynElement scopeBox)
        {
            return ScopedElements<DB.View>(
                scopeBox, DB.BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP);
        }

        /// <summary>
        /// Gets all Levels that use the supplied Scope Box.
        /// </summary>
        /// <param name="scopeBox">The Scope Box.</param>
        /// <returns name="levels">Levels using the Scope Box.</returns>
        /// <search>Revit.ScopeBox.Levels</search>
        [NodeCategory("Query")]
        public static IList<DynElement?> Levels(DynElement scopeBox)
        {
            return ScopedElements<DB.Level>(
                scopeBox, DB.BuiltInParameter.DATUM_VOLUME_OF_INTEREST);
        }

        /// <summary>
        /// Gets all Grids that use the supplied Scope Box.
        /// </summary>
        /// <param name="scopeBox">The Scope Box.</param>
        /// <returns name="grids">Grids using the Scope Box.</returns>
        /// <search>Revit.ScopeBox.Grids</search>
        [NodeCategory("Query")]
        public static IList<DynElement?> Grids(DynElement scopeBox)
        {
            return ScopedElements<DB.Grid>(
                scopeBox, DB.BuiltInParameter.DATUM_VOLUME_OF_INTEREST);
        }
    }
}
