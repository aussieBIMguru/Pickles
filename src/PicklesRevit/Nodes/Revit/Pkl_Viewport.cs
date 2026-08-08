using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Viewports.
    /// </summary>
    public class Pkl_Viewport
    {
        internal Pkl_Viewport() { }

        /// <summary>
        /// Places Views or Schedules on Sheets at specified Points.
        /// 
        /// Unlike other nodes, this can exclude other Element's impact on how this point is determined.
        /// </summary>
        /// <param name="views">The Views to place.</param>
        /// <param name="sheets">The Sheets to place the Views on.</param>
        /// <param name="points">The Points to place the Views at (centre point for Views, placement point for Schedules).</param>
        /// <param name="viewportType">The ViewportType to apply to the Viewports.</param>
        /// <param name="excludeElements">Excludes other Elements by temporarily hiding them during the calculation.</param>
        /// <returns name="viewports">The created Viewports (null if unsuccessful).</returns>
        /// <search>Revit.Viewport.Create</search>
        [NodeCategory("Create")]
        public static List<DynElement?> Create(List<DynView> views, List<DynSheet> sheets, List<DynPoint> points,
            DynElement viewportType, bool excludeElements = true)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Longest lacing the points
            points = points.Ext_LaceLongest(views.Count).ToList();

            // Notify user if mismatch in input sizes
            if (views.Count != sheets.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
            }

            // Isolate Element (optional, null sentinels do not isolate in function)
            DB.ElementId? isolateId = excludeElements ? doc.Ext_CollectByClass<DB.BasePoint>().First().Id : null;

            // Viewport type (only applied if valid for Viewports)
            DB.ElementId viewportTypeId = null;

            if (viewportType.InternalElement is DB.ElementType dbElementType &&
                dbElementType.get_Parameter(DB.BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM)
                ?.AsString() == "Viewport")
            {
                viewportTypeId = dbElementType.Id;
            }
            else
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("ViewportType was not valid.\n\n" +
                    "Default type will be used for created Viewports.");
            }

            List<DynElement?> viewports = new();

            doc.Ext_EnsureTransaction();

            // Isolate viewport view to get actual centre
            if (excludeElements)
            {
                foreach (DynView view in views)
                {
                    (view.InternalElement as DB.View)?.IsolateElementTemporary(isolateId);
                }
                
                doc.Regenerate();
            }

            int errorCount = 0;

            for (int i = 0; i < Math.Min(views.Count, sheets.Count); i++)
            {
                DynElement viewport = CreateViewport(
                    views[i],
                    sheets[i],
                    points[i],
                    viewportTypeId,
                    doc);

                viewports.Add(viewport);
                if (viewport == null) { errorCount++; }
            }

            // Unisolate view contents
            if (excludeElements)
            {
                doc.Regenerate();

                foreach (DynView view in views)
                {
                    (view.InternalElement as DB.View)?.DisableTemporaryViewMode(DB.TemporaryViewMode.TemporaryHideIsolate);
                }
            }

            doc.Ext_TransactionDone();

            if (errorCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Some Viewports could not be created.\n\n" +
                    "This is either due to invalid inputs, or a View may already be placed.");
            }

            return viewports;
        }

        /// <summary>
        /// Helper method to create a Viewport.
        /// </summary>
        /// <param name="view">The Dynamo View.</param>
        /// <param name="sheet">The Dynamo Sheet.</param>
        /// <param name="point">The Dynamo Point.</param>
        /// <param name="typeId">The ElementId of the Viewport type (tries to set).</param>
        /// <param name="doc">The Document.</param>
        /// <returns></returns>
        private static DynElement? CreateViewport(DynView view, DynSheet sheet, DynPoint point,
            DB.ElementId typeId, DB.Document doc = null)
        {
            // Invalid inputs check
            if (view.InternalElement is not DB.View dbView
                || sheet.InternalElement is not DB.ViewSheet dbSheet
                || point?.ToXyz() is not DB.XYZ dbPoint)
            {
                return null;
            }

            doc ??= dbView.Document;

            // Catch if it's a Schedule, no isolation routine needed if so
            if (dbView is DB.ViewSchedule dbSchedule)
            {
                return DB.ScheduleSheetInstance.Create(doc,
                    dbSheet.Id, dbSchedule.Id, dbPoint).Ext_ToDynElement(true);
            }

            // Ensure we can place the viewport
            if (!DB.Viewport.CanAddViewToSheet(doc, dbSheet.Id, dbView.Id)) { return null; }

            // Create a Viewport if it's not a Schedule being placed
            DB.Viewport viewport = DB.Viewport.Create(dbView.Document,
                dbSheet.Id, dbView.Id, dbPoint);

            if (typeId.Ext_IsValid())
            {
                viewport.ChangeTypeId(typeId);
            }

            // Postcorrect
            viewport.SetBoxCenter(dbPoint);

            // Return the Viewport
            return viewport.Ext_ToDynElement(true);
        }

        /// <summary>
        /// Gets the Viewports centre points.
        /// 
        /// Unlike other nodes, this can exclude other Element's impact on how this point is determined.
        /// </summary>
        /// <param name="viewports">The Viewports.</param>
        /// <param name="excludeElements">Excludes other Elements by temporarily hiding them during the calculation.</param>
        /// <returns name="point">The Viewports View.</returns>
        /// <search>Revit.Viewport.GetCentrePoint</search>
        [NodeCategory("Action")]
        public static List<DynPoint?> GetCentrePoint(List<DynViewport> viewports, bool excludeElements = true)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            List<DynPoint?> centrePoints = new();

            // Hide all Elements in View (needs dry Transaction)
            if (excludeElements)
            {
                DB.ElementId isolateId = doc.Ext_CollectByClass<DB.BasePoint>().First().Id;

                TransactionManager.Instance.ForceCloseTransaction();

                // NB: Use Revit Transaction for rollback behavior
                using (var t = new DB.Transaction(doc, "Pickle: Viewports.GetCentrePoint"))
                {
                    t.Start();

                    foreach (DynElement viewport in viewports)
                    {
                        if (viewport.InternalElement is DB.Viewport dbViewport)
                        {
                            // Hide all Elements in Viewport's View
                            DB.View viewportView = dbViewport.ViewId.Ext_GetElement<DB.View>(doc);
                            viewportView.IsolateElementTemporary(isolateId);

                            // Get the true Viewport box centre
                            DynPoint actualCentre = dbViewport.GetBoxCenter().ToPoint(true);
                            centrePoints.Add(actualCentre);
                        }
                        else
                        {
                            centrePoints.Add(null);
                        }
                    }

                    t.Ext_RollbackIfOpen();
                }
            }
            else
            {
                foreach (DynElement viewport in viewports)
                {
                    if (viewport.InternalElement is DB.Viewport dbViewport)
                    {
                        // Get the default Viewport box centre
                        DynPoint boxCentre = dbViewport.GetBoxCenter().ToPoint(true);
                        centrePoints.Add(boxCentre);
                    }
                    else
                    {
                        centrePoints.Add(null);
                    }
                }
            }

            return centrePoints;
        }

        /// <summary>
        /// Gets the View of a Viewport.
        /// </summary>
        /// <param name="viewport">The Viewport to get the View for.</param>
        /// <returns name="view">The Viewports View.</returns>
        /// <search>Revit.Viewport.View</search>
        [NodeCategory("Query")]
        public static DynElement? View(DynViewport viewport)
        {
            if (viewport.InternalElement is DB.Viewport dbViewport)
            {
                return dbViewport.ViewId
                    .Ext_GetDynamoElement(dbViewport.Document, true);
            }
            return null;
        }

        /// <summary>
        /// Gets the Sheet of a Viewport.
        /// </summary>
        /// <param name="viewport">The Viewport to get the Sheet for.</param>
        /// <returns name="view">The Viewports Sheet.</returns>
        /// <search>Revit.Viewport.Sheet</search>
        [NodeCategory("Query")]
        public static DynElement? Sheet(DynViewport viewport)
        {
            if (viewport.InternalElement is DB.Viewport dbViewport)
            {
                return dbViewport.SheetId
                    .Ext_GetDynamoElement(dbViewport.Document, true);
            }
            return null;
        }
    }
}