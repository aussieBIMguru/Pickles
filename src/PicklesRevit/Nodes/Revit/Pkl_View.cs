
using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to View.
    /// </summary>
    public class Pkl_View
    {
        internal Pkl_View() { }

        /// <summary>
        /// Creates dependent Views from a View using a list of names.
        /// 
        /// If naming fails, the Views will still be created.
        /// </summary>
        /// <param name="view">The source View.</param>
        /// <param name="names">Names for the dependent Views.</param>
        /// <returns name="views">Created dependent Views.</returns>
        /// <returns name="success">Whether each dependent View was created.</returns>
        /// <search>Revit.View.CreateDependents</search>
        [NodeCategory("Create")]
        [MultiReturn("views", "success")]
        public static Dictionary<string, object> CreateDependents(
            DynView view,
            List<string> names)
        {
            var createdViews = new List<DynElement?>();
            var success = new List<bool>();
            DB.View dbView = view.InternalElement as DB.View;

            // Cancel early if view cannot be duplicated
            if (!dbView.CanViewBeDuplicated(DB.ViewDuplicateOption.AsDependent))
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("View cannot be duplicated as dependent.");
                
                return new Dictionary<string, object>
                {
                    { "views", Enumerable.Repeat<DynElement>(null, names.Count).ToList() },
                    { "success", Enumerable.Repeat(false, names.Count).ToList() }
                };
            }

            TransactionManager.Instance.ForceCloseTransaction();
            DB.Document doc = dbView.Document;
            int nameFailureCount = 0;

            using (var t = new DB.Transaction(doc, "Pickle: Views.CreateDependents"))
            {
                t.Start();

                foreach (var name in names)
                {
                    // Create dependent view
                    DB.View newView = dbView
                        .Duplicate(DB.ViewDuplicateOption.AsDependent)
                        .Ext_GetElement<DB.View>(doc);

                    // Try to rename, catch if fails
                    try
                    {
                        newView.Name = name;
                    }
                    catch
                    {
                        nameFailureCount++;
                    }

                    // Add outputs
                    createdViews.Add(newView?.Ext_ToDynElement(true));
                    success.Add(newView != null);
                }

                t.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Notify user if rename issues occured
            if (nameFailureCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Some Views could not be renamed.\n\n" +
                    "Default Revit naming used.");
            }

            // Return outputs
            return new Dictionary<string, object>
            {
                { "views", createdViews },
                { "success", success }
            };
        }

        /// <summary>
        /// Sets a View's Phase.
        /// </summary>
        /// <param name="view">The View to change the Phase of.</param>
        /// <param name="phase">The Phase to apply.</param>
        /// <returns name="success">Was the View changed.</returns>
        /// <search>Revit.View.SetPhase</search>
        [NodeCategory("Action")]
        public static bool SetPhase(DynView view, DynElement phase)
        {
            // Ensure we were given a Revit phase
            DB.ElementId setPhaseId = null;
            if (phase.InternalElement is DB.Phase revitPhase) { setPhaseId = revitPhase.Id; }

            if (setPhaseId is null)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Invalid phase provided.");
                return false;
            }
            
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            bool success = false;

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(doc, "Pickle: View.SetPhase"))
            {
                transaction.Start();

                if (view.InternalElement is DB.View dbView)
                {
                    // Get phase parameter
                    DB.Parameter parameter = view.InternalElement
                        .Ext_GetBuiltInParameter(DB.BuiltInParameter.VIEW_PHASE);

                    // Ensure we got a parameter
                    if (parameter != null)
                    {
                        // Same phase = no change needed
                        if (parameter.AsElementId() == setPhaseId)
                        {
                            success = true;
                        }
                        // Try to change phase
                        else
                        {
                            try
                            {
                                parameter.Set(setPhaseId);
                                success = true;
                            }
                            catch {; }
                        }
                    }
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            return success;
        }

        /// <summary>
        /// Returns the crop region boundary of a View as a Dynamo PolyCurve.
        /// </summary>
        /// <param name="view">The View.</param>
        /// <returns name="polycurve">The crop region boundary as a Dynamo PolyCurve.</returns>
        /// <search>Revit.View.GetCropRegion</search>
        [NodeCategory("Action")]
        public static DynPolyCurve? GetCropRegion(DynView view)
        {
            if (view.InternalElement is DB.View dbView)
            {
                // Get the curveloops (generally only 1)
                DB.ViewCropRegionShapeManager cropManager = dbView.GetCropRegionShapeManager();
                IList<DB.CurveLoop> cropLoops = cropManager.GetCropShape();

                // Return null if no valid curves
                if (cropLoops == null || cropLoops.Count == 0)
                {
                    return null;
                }

                // Usually only one loop exists
                var curves = cropLoops[0]
                    .Select(x => x.ToProtoType())
                    .Cast<DynCurve>()
                    .ToList();

                // Return the PolyCurve
                return DynPolyCurve.ByJoinedCurves(curves, 0.001, false, 0.0);
            }

            return null;
        }

        /// <summary>
        /// Sets a View's crop region from a Dynamo PolyCurve.
        /// </summary>
        /// <param name="view">The View.</param>
        /// <param name="polycurve">The Dynamo PolyCurve defining the crop boundary.</param>
        /// <param name="visible">Sets the crop region visible.</param>
        /// <returns name="success">Whether the crop region was updated.</returns>
        /// <search>Revit.View.SetCropShape</search>
        [NodeCategory("Action")]
        public static bool SetCropRegion(DynView view, DynPolyCurve polycurve, bool visible = true)
        {
            // Ensure we have a View and crop region manager
            if (view.InternalElement is not DB.View dbView) return false;
            
            // Guard against scope box assignment
            if (dbView.get_Parameter(DB.BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP) != null)
            {
                DB.ElementId scopeBoxId = dbView.get_Parameter(
                    DB.BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP)
                    .AsElementId();

                if (scopeBoxId.Ext_IsValid())
                {
                    WARNING_TYPE.DEFAULT.Ext_Raise("The View has a Scope box applied.\n\n" +
                        "Crop cannot be changed.");
                    return false;
                }
            }

            DB.ViewCropRegionShapeManager cropManager = dbView.GetCropRegionShapeManager();
            if (cropManager == null) return false;

            // Construct the Revit CurveLoop to apply
            var curveLoop = new DB.CurveLoop();

            foreach (var curve in polycurve.Curves())
            {
                curveLoop.Append(curve.ToRevitType());
            }

            // Close any active Dynamo transactions
            TransactionManager.Instance.ForceCloseTransaction();

            DB.Document doc = dbView.Document;
            bool success = false;

            using (var transaction = new DB.Transaction(doc, "Pickle: Views.SetCropRegion"))
            {
                transaction.Start();

                // Try to set crop box
                try
                {
                    dbView.CropBoxActive = true;
                    dbView.CropBoxVisible = visible;
                    cropManager.SetCropShape(curveLoop);

                    transaction.Commit();
                    success = true;
                }
                catch (Exception ex)
                {
                    WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
                    transaction.RollBack();
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            return success;
        }

        /// <summary>
        /// Returns the Y axis aligned bounding box of a View's section box.
        /// </summary>
        /// <param name="view">The View.</param>
        /// <returns name="boundingBox">Was section box as a Dynamo BoundingBox.</returns>
        /// <search>Revit.View.GetSectionBox</search>
        [NodeCategory("Action")]
        public static DynBb? GetSectionBox(DynView3D view)
        {
            if (view.InternalElement is DB.View3D view3d)
            {
                var sectionBox = view3d.GetSectionBox();
                if (sectionBox == null) { return null; }

                DB.Transform t = sectionBox.Transform;
                DB.XYZ min = t.OfPoint(sectionBox.Min);
                DB.XYZ max = t.OfPoint(sectionBox.Max);

                return DynBb.ByCorners(
                    min.ToPoint(),
                    max.ToPoint());
            }

            return null;
        }

        /// <summary>
        /// Returns all parent/primary Views of this View, as well as all further parents.
        /// </summary>
        /// <param name="view">The View.</param>
        /// <param name="includeView">Include the View in its own ancestry.</param>
        /// <returns name="views">The ancestry of Views.</returns>
        /// <search>Revit.View.GetAncestry</search>
        [NodeCategory("Action")]
        public static IList<DynElement?> GetAncestry(DynView view, bool includeView = false)
        {
            if (view.InternalElement is DB.View dbView)
            {
                return dbView.Ext_GetAncestryViewIds()
                    .Ext_ToDynamoElements(true, dbView.Document);
            }
            return new List<DynElement?>();
        }

        /// <summary>
        /// Applies a View Template to provided View.
        /// </summary>
        /// <param name="view">The View to change.</param>
        /// <param name="viewTemplate">The View template to apply.</param>
        /// <returns name="success">Was the template applied (or applied already).</returns>
        /// <search>Revit.View.SetViewTemplate</search>
        [NodeCategory("Action")]
        public static bool SetViewTemplate(DynView view, DynView viewTemplate)
        {
            // Ensure view template
            DB.ElementId viewTemplateId = null;

            if (viewTemplate.InternalElement is DB.View dbTemplate
                && dbTemplate.IsTemplate)
            {
                viewTemplateId = dbTemplate.Id;
            }
            else
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Invalid View Template provided.");
                return false;
            }

            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            bool success = false;

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(doc, "Pickle: View.SetViewTemplate"))
            {
                transaction.Start();

                if (view.InternalElement is DB.View dbView)
                {
                    // Already templated
                    if (dbView.ViewTemplateId == viewTemplateId)
                    {
                        success = true;
                    }
                    else
                    {
                        // Try to set template
                        try
                        {
                            dbView.ViewTemplateId = viewTemplateId;
                            success = true;
                        }
                        catch {; }
                    }
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            return success;
        }

        /// <summary>
        /// Removes the View Template from provided View.
        /// </summary>
        /// <param name="view">The View to change.</param>
        /// <returns name="success">Was the template removed (or none already).</returns>
        /// <search>Revit.View.RemoveViewTemplate</search>
        [NodeCategory("Action")]
        public static bool RemoveViewTemplate(DynView view)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            bool success = false;

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(doc, "Pickle: Views.RemoveTemplate"))
            {
                transaction.Start();

                if (view.InternalElement is DB.View dbView)
                {
                    // Already no template
                    if (dbView.ViewTemplateId.Ext_IsInValid())
                    {
                        success = true;
                    }
                    else
                    {
                        // Try to set to no template
                        try
                        {
                            dbView.ViewTemplateId = DB.ElementId.InvalidElementId;
                            success = true;
                        }
                        catch {; }
                    }
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            return success;
        }

        /// <summary>
        /// Opens a View.
        /// </summary>
        /// <param name="view">The View to open.</param>
        /// <returns name="success">Was the View opened.</returns>
        /// <search>Revit.View.OpenView</search>
        [NodeCategory("Action")]
        public static bool OpenView(DynView view)
        {
            if (view.InternalElement is DB.View dbView)
            {
                RUI.UIDocument uiDoc = DocumentManager.Instance.CurrentUIDocument;

                // Make sure transaction is clear
                TransactionManager.Instance.EnsureInTransaction(dbView.Document);
                TransactionManager.Instance.ForceCloseTransaction();

                try
                {
                    uiDoc.RequestViewChange(dbView);
                    return true;
                }
                catch {; }
            }

            return false;
        }

        /// <summary>
        /// Attempts to get a View's dependent Views.
        /// </summary>
        /// <param name="view">The View to get the dependent Views for.</param>
        /// <returns name="views">The dependent Views, if any.</returns>
        /// <search>Revit.View.GetDependentViews</search>
        [NodeCategory("Action")]
        public static IEnumerable<DynElement> GetDependentViews(DynView view)
        {
            if (view.InternalElement is DB.View dbView)
            {
                return dbView.GetDependentViewIds()
                    .Select(i => i.Ext_GetDynamoElement(dbView.Document, true));
            }
            return new List<DynElement>();
        }

        /// <summary>
        /// Returns the sketch plane of a View as a Dynamo Plane.
        /// </summary>
        /// <param name="view">The View.</param>
        /// <returns name="plane">The View sketch plane as a Dynamo Plane.</returns>
        /// <search>Revit.View.GetPlane</search>
        [NodeCategory("Action")]
        public static DynPlane? GetPlane(DynView view)
        {
            if (view.InternalElement is DB.View dbView)
            {
                DB.SketchPlane sketchPlane = dbView.SketchPlane;

                if (sketchPlane == null)
                {
                    return null;
                }

                DB.Plane plane = sketchPlane.GetPlane();

                return DynPlane.ByOriginNormal(
                    plane.Origin.ToPoint(),
                    plane.Normal.ToVector());
            }

            return null;
        }

        /// <summary>
        /// Attempts to get the applied View template.
        /// </summary>
        /// <param name="view">The View to get the View template for.</param>
        /// <returns name="viewTemplate">The applied template, if any.</returns>
        /// <search>Revit.View.GetViewTemplate</search>
        [NodeCategory("Action")]
        public static DynElement? GetViewTemplate(DynView view)
        {
            if (view.InternalElement is DB.View dbView)
            {
                return dbView.ViewTemplateId
                    .Ext_GetDynamoElement(dbView.Document, true);
            }
            return null;
        }

        /// <summary>
        /// Attempts to get the ViewType of the View (e.g. FloorPlan).
        /// 
        /// Note this is not the same as the View's ViewFamilyType (aka Type).
        /// </summary>
        /// <param name="view">The View to get the ViewType for.</param>
        /// <returns name="viewType">The ViewType as a string.</returns>
        /// <search>Revit.View.ViewType</search>
        [NodeCategory("Query")]
        public static string? ViewType(DynView view)
        {
            if (view.InternalElement is DB.View dbView)
            {
                return dbView.ViewType.ToString();
            }
            return null;
        }

        /// <summary>
        /// Attempts to get a View's related Level.
        /// </summary>
        /// <param name="view">The View to get the Level for.</param>
        /// <returns name="level">The Level, if any.</returns>
        /// <search>Revit.View.GetLevel</search>
        [NodeCategory("Action")]
        public static DynElement? GetLevel(DynView view)
        {
            if (view.InternalElement is DB.View dbView)
            {
                return dbView.GenLevel.Ext_ToDynElement(true);
            }
            return null;
        }

        /// <summary>
        /// Attempts to get the Phase associated with a View.
        /// </summary>
        /// <param name="view">The View to get the Phase for.</param>
        /// <returns name="phase">The Phase, if any.</returns>
        /// <search>Revit.View.GetPhase</search>
        [NodeCategory("Action")]
        public static DynElement? GetPhase(DynView view)
        {
            // Get parameter, null check it
            DB.Parameter parameter = view.InternalElement
                .Ext_GetBuiltInParameter(DB.BuiltInParameter.VIEW_PHASE);
            if (parameter is null) { return null; }

            // Return the value
            return parameter.AsElementId()
                .Ext_GetDynamoElement(view.InternalElement.Document, true);
        }

        /// <summary>
        /// Attempts to get the first Sheet a View is placed on.
        /// </summary>
        /// <param name="view">The View to get the Sheet for.</param>
        /// <returns name="sheet">The Sheet, if any.</returns>
        /// <search>Revit.View.GetSheet</search>
        [NodeCategory("Action")]
        public static DynElement? GetSheet(DynView view)
        {
            // Get parameter, null check it
            DB.Parameter parameter = view.InternalElement
                .Ext_GetBuiltInParameter(DB.BuiltInParameter.VIEWER_SHEET_NUMBER);
            if (parameter is null || parameter.AsString().Ext_HasNoChars()) { return null; }

            // Parameter filter
            DB.FilterRule filterRule = DB.ParameterFilterRuleFactory.CreateEqualsRule(
                new DB.ElementId(DB.BuiltInParameter.SHEET_NUMBER),
                parameter.AsString());
            var filter = new DB.ElementParameterFilter(filterRule);

            // Return the value
            return view.InternalElement.Document.Ext_Collector()
                .OfCategory(DB.BuiltInCategory.OST_Sheets)
                .WherePasses(filter)
                .FirstOrDefault()?
                .Ext_ToDynElement(true);
        }

        /// <summary>
        /// Attempts to get the primary View that this View depends on.
        /// </summary>
        /// <param name="view">The View to get the primary View for.</param>
        /// <returns name="primaryView">The View, if any.</returns>
        /// <search>Revit.View.PrimaryView</search>
        [NodeCategory("Query")]
        public static DynElement? PrimaryView(DynView view)
        {
            if (view.InternalElement is DB.View dbView)
            {
                return dbView.GetPrimaryViewId()
                    .Ext_GetDynamoElement(dbView.Document, true);
            }
            return null;
        }

        /// <summary>
        /// Attempts to get the parent View that this View is called out from.
        /// </summary>
        /// <param name="view">The View to get the parent View for.</param>
        /// <returns name="parentView">The View, if any.</returns>
        /// <search>Revit.View.ParentView</search>
        [NodeCategory("Query")]
        public static DynElement? ParentView(DynView view)
        {
            // Get parameter, null check it
            DB.Parameter parameter = view.InternalElement
                .Ext_GetBuiltInParameter(DB.BuiltInParameter.SECTION_PARENT_VIEW_NAME);
            if (parameter is null) { return null; }

            // Return the value
            return parameter.AsElementId()
                .Ext_GetDynamoElement(view.InternalElement.Document, true);
        }
    }
}