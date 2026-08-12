namespace Pickles.Extensions
{
    internal static class Ext_DBView
    {
        /// <summary>
        /// Returns all ancestor ViewIds that this View depends on.
        /// </summary>
        /// <param name="view">The View.</param>
        /// <param name="includeView">Include the View in its ancestry.</param>
        /// <returns>A list of ancestor ViewIds.</returns>
        public static IEnumerable<DB.ElementId> Ext_GetAncestryViewIds(this DB.View view, bool includeView = false)
        {
            var ancestryViewIds = new List<DB.ElementId> { };
            if (includeView) { ancestryViewIds.Add(view.Id); }
            var visited = new HashSet<DB.ElementId> { view.Id };

            DB.View currentView = view;
            DB.Document doc = view.Document;

            while (true)
            {
                DB.Parameter parameter = currentView.Ext_GetBuiltInParameter(
                    DB.BuiltInParameter.SECTION_PARENT_VIEW_NAME);
                if (parameter == null) break;

                DB.ElementId parentViewId = parameter.AsElementId();
                if (!parentViewId.Ext_IsValid()) break;

                if (!visited.Add(parentViewId)) break;
                ancestryViewIds.Add(parentViewId);

                currentView = parentViewId.Ext_GetElement<DB.View>(doc);
                if (currentView == null) break;
            }

            var primaryViewId = view.GetPrimaryViewId();

            if (primaryViewId.Ext_IsValid() && visited.Add(primaryViewId))
            {
                ancestryViewIds.Add(primaryViewId);
            }

            return ancestryViewIds;
        }
    }
}
