namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Auditing a model.
    /// </summary>
    public class Pkl_Audit
    {
        internal Pkl_Audit() { }

        private const string KEY_UNUSED = "unused";
        private const string KEY_COUNT = "deletedCount";

        private static Dictionary<string, object> EmptyOutput()
        {
            return new Dictionary<string, object>()
            {
                { KEY_UNUSED, new List<DynElement>() },
                { KEY_COUNT, 0 }
            };
        }

        private static Dictionary<string, object> ProcessUnused<T>(
            DB.Document doc, 
            IList<T> unusedElements,
            bool purgeUnused) where T : DB.Element
        {
            var output = new Dictionary<string, object>()
            {
                { KEY_UNUSED, new List<DynElement>() },
                { KEY_COUNT, 0 }
            };

            if (purgeUnused)
            {
                int deletedCount = 0;

                doc.Ext_EnsureTransaction();

                foreach (T element in unusedElements)
                {
                    try
                    {
                        doc.Delete(element.Id);
                        deletedCount++;
                    }
                    catch
                    {
                        // Ignore elements that cannot be deleted.
                    }
                }

                doc.Ext_TransactionDone();

                output[KEY_COUNT] = deletedCount;
            }
            else
            {
                output[KEY_UNUSED] = unusedElements
                    .Ext_ToDynamoElements(true);
            }

            return output;
        }

        /// <summary>
        /// Gets all views that are not placed on a sheet or referenced by another used view,
        /// with the option to purge them.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <param name="purgeUnused">Whether to delete unused views.</param>
        /// <returns name="unused">The unused views (if not deleted).</returns>
        /// <returns name="deletedCount">The number of deleted views.</returns>
        /// <search>Revit.Audit.UnusedViews</search>
        [NodeCategory("Action")]
        [MultiReturn("unused", "deletedCount")]
        public static Dictionary<string, object> UnusedViews(
            [DefaultArgument("null")] object? docOrLinkInstance = null, bool purgeUnused = false)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return EmptyOutput();
            }

            // Collect candidate Views
            DB.Document doc = docHelper.Document;

            IList<DB.View> views = doc.Ext_CollectViews()
                .Where(v => !v.IsTemplate)
                .ToList();

            // Collect View Ids used by placed Views and their ancestry
            HashSet<DB.ElementId> usedViewIds = [];

            foreach (DB.Viewport viewport in doc.Ext_CollectByClass<DB.Viewport>())
            {
                DB.View? view = viewport.ViewId.Ext_GetElement<DB.View>(doc);

                if (view == null)
                {
                    continue;
                }

                usedViewIds.UnionWith(
                    view.Ext_GetAncestryViewIds(includeView: true)
                );
            }

            // Get unused, process the outcome
            List<DB.View> unusedViews = views
                .Where(v => !usedViewIds.Contains(v.Id))
                .ToList();
            return ProcessUnused(doc, unusedViews, purgeUnused);
        }

        /// <summary>
        /// Gets all View Templates that are not assigned to any view or View Family Type,
        /// with the option to purge them.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <param name="purgeUnused">Whether to delete unused View Templates.</param>
        /// <returns name="unused">The unused View Templates (if not deleted).</returns>
        /// <returns name="deletedCount">The number of deleted View Templates.</returns>
        /// <search>Revit.Audit.UnusedViewTemplates</search>
        [NodeCategory("Action")]
        [MultiReturn("unused", "deletedCount")]
        public static Dictionary<string, object> UnusedViewTemplates(
            [DefaultArgument("null")] object? docOrLinkInstance = null,
            bool purgeUnused = false)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return EmptyOutput();
            }

            // Collect Views and View Family Types
            DB.Document doc = docHelper.Document;
            IList<DB.View> views = doc.Ext_CollectByClass<DB.View>();
            IList<DB.ViewFamilyType> viewFamilyTypes = doc.Ext_CollectByClass<DB.ViewFamilyType>();

            // Collect used View Templates
            HashSet<DB.ElementId> usedTemplateIds = [];

            foreach (DB.View view in views)
            {
                if (!view.IsTemplate && view.ViewTemplateId != DB.ElementId.InvalidElementId)
                {
                    usedTemplateIds.Add(view.ViewTemplateId);
                }
            }

            // Collect View Family Type default templates
            foreach (DB.ViewFamilyType viewFamilyType in viewFamilyTypes)
            {
                if (viewFamilyType.DefaultTemplateId != DB.ElementId.InvalidElementId)
                {
                    usedTemplateIds.Add(viewFamilyType.DefaultTemplateId);
                }
            }

            // Get unused, process the outcome
            List<DB.View> unusedTemplates = views
                .Where(v => v.IsTemplate && !usedTemplateIds.Contains(v.Id))
                .ToList();
            return ProcessUnused(doc, unusedTemplates, purgeUnused);
        }

        /// <summary>
        /// Gets all filter elements that are not applied to any view, with the option to purge them.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <param name="purgeUnused">Whether to delete unused filters.</param>
        /// <returns name="unused">The unused filters (if not deleted).</returns>
        /// <returns name="deletedCount">The number of deleted Filters.</returns>
        /// <search>Revit.Audit.UnusedFilters</search>
        [NodeCategory("Action")]
        [MultiReturn("unused", "deletedCount")]
        public static Dictionary<string, object> UnusedFilters(
            [DefaultArgument("null")] object? docOrLinkInstance = null, bool purgeUnused = false)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return EmptyOutput();
            }

            // Collect Views and Filters
            DB.Document doc = docHelper.Document;
            IList<DB.View> views = doc.Ext_CollectByClass<DB.View>();
            IList<DB.FilterElement> filters = doc.Ext_CollectByClass<DB.FilterElement>();

            // Collect used Filters
            HashSet<DB.ElementId> usedFilterIds = [];

            foreach (DB.View view in views)
            {
                try
                {
                    foreach (DB.ElementId id in view.GetFilters())
                    {
                        usedFilterIds.Add(id);
                    }
                }
                catch {; }
            }

            // Get unused, process the outcome
            List<DB.FilterElement> unusedFilters = filters
                .Where(f => !usedFilterIds.Contains(f.Id))
                .ToList();
            return ProcessUnused(doc, unusedFilters, purgeUnused);
        }

        /// <summary>
        /// Gets all ElevationMarkers that contain no elevations, with the option to purge them.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <param name="purgeUnused">Whether to delete unused ElevationMarkers.</param>
        /// <returns name="unused">The unused ElevationMarkers (if not deleted).</returns>
        /// <returns name="deletedCount">The number of deleted ElevationMarkers.</returns>
        /// <search>Revit.Audit.UnusedElevationMarkers</search>
        [NodeCategory("Action")]
        [MultiReturn("unused", "deletedCount")]
        public static Dictionary<string, object> UnusedElevationMarkers(
            [DefaultArgument("null")] object? docOrLinkInstance = null, bool purgeUnused = false)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return EmptyOutput();
            }

            DB.Document doc = docHelper.Document;

            // Get unused, process the outcome
            List<DB.ElevationMarker> unusedMarkers = doc
                .Ext_CollectByClass<DB.ElevationMarker>()
                .Where(m => !m.HasElevations())
                .ToList();
            return ProcessUnused(doc, unusedMarkers, purgeUnused);
        }

        /// <summary>
        /// Gets all unused regular schedules, with the option to purge them.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <param name="purgeUnused">Whether to delete unused schedules.</param>
        /// <returns name="unused">The unused schedules (if not deleted).</returns>
        /// <returns name="deletedCount">The number of deleted schedules.</returns>
        /// <search>Revit.Audit.UnusedSchedules</search>
        [NodeCategory("Action")]
        [MultiReturn("unused", "deletedCount")]
        public static Dictionary<string, object> UnusedSchedules(
            [DefaultArgument("null")] object? docOrLinkInstance = null,
            bool purgeUnused = false)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return EmptyOutput();
            }

            // Collect Schedules and Schedule Instances
            DB.Document doc = docHelper.Document;
            IList<DB.ViewSchedule> schedules = doc.Ext_CollectByClass<DB.ViewSchedule>();
            IList<DB.ScheduleSheetInstance> instances = doc.Ext_CollectByClass<DB.ScheduleSheetInstance>();

            // Collect used Schedule Ids
            HashSet<DB.ElementId> usedScheduleIds = [];

            foreach (DB.ScheduleSheetInstance instance in instances)
            {
                DB.ViewSchedule? schedule =
                    doc.GetElement(instance.ScheduleId) as DB.ViewSchedule;

                if (schedule == null)
                {
                    continue;
                }

                if (schedule.Definition.IsKeySchedule ||
                    schedule.Name.Contains("<Revision"))
                {
                    continue;
                }

                usedScheduleIds.Add(schedule.Id);
            }

            // Get unused, process the outcome
            List<DB.ViewSchedule> unusedSchedules = schedules
                .Where(s =>
                    !s.IsTemplate &&
                    !s.Definition.IsKeySchedule &&
                    !s.Name.Contains("<Revision") &&
                    !usedScheduleIds.Contains(s.Id))
                .ToList();
            return ProcessUnused(doc, unusedSchedules, purgeUnused);
        }

        /// <summary>
        /// Gets all Scope Boxes that are not assigned to any View, Level, or Grid,
        /// with the option to purge them.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <param name="purgeUnused">Whether to delete unused Scope Boxes.</param>
        /// <returns name="unused">The unused Scope Boxes (if not deleted).</returns>
        /// <returns name="deletedCount">The number of deleted Scope Boxes.</returns>
        /// <search>Revit.Audit.UnusedScopeBoxes</search>
        [NodeCategory("Action")]
        [MultiReturn("unused", "deletedCount")]
        public static Dictionary<string, object> UnusedScopeBoxes(
            [DefaultArgument("null")] object? docOrLinkInstance = null,
            bool purgeUnused = false)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return EmptyOutput();
            }

            // Get the related Document
            DB.Document doc = docHelper.Document;

            // Collect Scope Boxes
            IList<DB.Element> scopeBoxes = doc.Ext_CollectByCategory(
                DB.BuiltInCategory.OST_VolumeOfInterest);

            // Collect used Scope Box Ids
            HashSet<DB.ElementId> usedScopeBoxIds = [];

            // Views
            foreach (DB.View view in doc.Ext_CollectByClass<DB.View>())
            {
                DB.Parameter? parameter = view.Ext_GetBuiltInParameter(
                    DB.BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP);

                DB.ElementId? id = parameter?.AsElementId();

                if (id.Ext_IsValid())
                {
                    usedScopeBoxIds.Add(id);
                }
            }

            // Levels
            foreach (DB.Level level in doc.Ext_CollectByClass<DB.Level>())
            {
                DB.Parameter? parameter = level.Ext_GetBuiltInParameter(
                    DB.BuiltInParameter.DATUM_VOLUME_OF_INTEREST);

                DB.ElementId? id = parameter?.AsElementId();

                if (id.Ext_IsValid())
                {
                    usedScopeBoxIds.Add(id);
                }
            }

            // Grids
            foreach (DB.Grid grid in doc.Ext_CollectByClass<DB.Grid>())
            {
                DB.Parameter? parameter = grid.Ext_GetBuiltInParameter(
                    DB.BuiltInParameter.DATUM_VOLUME_OF_INTEREST);

                DB.ElementId? id = parameter?.AsElementId();

                if (id.Ext_IsValid())
                {
                    usedScopeBoxIds.Add(id);
                }
            }

            // Get unused, process the outcome
            List<DB.Element> unusedScopeBoxes = scopeBoxes
                .Where(sb => !usedScopeBoxIds.Contains(sb.Id))
                .ToList();
            return ProcessUnused(doc, unusedScopeBoxes, purgeUnused);
        }
    }
}