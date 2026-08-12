using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Views.
    /// </summary>
    public class Pkl_View
    {
        internal Pkl_View() { }

        /// <summary>
        /// List of ViewFamilies for ViewFamilyTypes that are Plan related.
        /// </summary>
        private static readonly HashSet<DB.ViewFamily> VIEWFAMILIES_PLAN = new HashSet<DB.ViewFamily>()
        {
            DB.ViewFamily.FloorPlan, DB.ViewFamily.CeilingPlan,
            DB.ViewFamily.StructuralPlan, DB.ViewFamily.AreaPlan
        };

        /// <summary>
        /// Creates a new Plan View with a given name.
        /// 
        /// If naming fails, the View will still be created.
        /// </summary>
        /// <param name="level">The Level of the plan View to create.</param>
        /// <param name="name">Names for the new View.</param>
        /// <param name="viewFamilyType">ViewFamilyType to use.</param>
        /// <param name="keepNameClashes">Retain Views that could not be named.</param>
        /// <returns name="view">Created Plan View.</returns>
        /// <search>Revit.View.CreatePlan</search>
        [NodeCategory("Create")]
        public static DynElement? CreatePlan(DynLevel level, string name,
            DynElement viewFamilyType, bool keepNameClashes = true)
        {
            if (level.InternalElement is not DB.Level dbLevel
                || viewFamilyType.InternalElement is not DB.ViewFamilyType dbVft
                || !VIEWFAMILIES_PLAN.Contains(dbVft.ViewFamily)
                || name.Ext_HasNoChars())
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            TransactionManager.Instance.ForceCloseTransaction();
            DB.Document doc = dbLevel.Document;
            DB.View? newView = null;

            // NB: Use Revit Transaction for rollback behavior
            using (var t = new DB.Transaction(doc, "Pickle: View.CreatePlan"))
            {
                t.Start();

                // Create plan view
                newView = DB.ViewPlan.Create(doc, dbVft.Id, dbLevel.Id);

                // Try to rename, catch if fails
                try
                {
                    newView.Name = name;
                }
                catch
                {
                    if (keepNameClashes)
                    {
                        WARNING_TYPE.DEFAULT.Ext_Raise("View could not be renamed.\n\n" +
                    "Default Revit naming used.");
                    }
                    else
                    {
                        WARNING_TYPE.DEFAULT.Ext_Raise("View could not be renamed.\n\n" +
                    "Creation was undone.");
                        t.RollBack();
                    }
                }

                t.Ext_CommitIfOpen();
            }

            // Return new view
            return newView.Ext_ToDynElement(true);
        }

        /// <summary>
        /// Creates dependent Views from a View using a list of names.
        /// 
        /// If naming fails, the Views will still be created.
        /// </summary>
        /// <param name="view">The source View.</param>
        /// <param name="names">Names for the dependent Views.</param>
        /// <param name="keepNameClashes">Retain Views that could not be named.</param>
        /// <returns name="views">Created dependent Views.</returns>
        /// <search>Revit.View.CreateDependents</search>
        [NodeCategory("Create")]
        public static List<DynElement?> CreateDependents(DynView view,
            List<string> names, bool keepNameClashes = true)
        {
            if (view.InternalElement is not DB.View dbView
                || !dbView.CanViewBeDuplicated(DB.ViewDuplicateOption.AsDependent))
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return Enumerable.Repeat<DynElement>(null, names.Count).ToList();
            }

            DB.Document doc = dbView.Document;
            List<DynElement?> createdViews = new();
            int nameFailureCount = 0;

            TransactionManager.Instance.ForceCloseTransaction();

            // NB: Use Revit Transaction for rollback behavior
            using (var t = new DB.Transaction(doc, "Pickle: View.CreateDependents"))
            {
                t.Start();

                foreach (string name in names)
                {
                    DB.View newView = dbView
                        .Duplicate(DB.ViewDuplicateOption.AsDependent)
                        .Ext_GetElement<DB.View>(doc);

                    try
                    {
                        newView.Name = name;
                    }
                    catch
                    {
                        nameFailureCount++;

                        if (!keepNameClashes)
                        {
                            doc.Delete(newView.Id);
                            createdViews.Add(null);
                            continue;
                        }
                    }

                    createdViews.Add(newView?.Ext_ToDynElement(true));
                }

                t.Ext_CommitIfOpen();
            }

            if (nameFailureCount > 0)
            {
                if (keepNameClashes)
                {
                    WARNING_TYPE.DEFAULT.Ext_Raise(
                        "Some Views could not be renamed.\n\n" +
                        "Default Revit naming used.");
                }
                else
                {
                    WARNING_TYPE.DEFAULT.Ext_Raise(
                        "Some Views could not be renamed and their creation was undone.");
                }
            }

            return createdViews;
        }

        /// <summary>
        /// Creates Elevation Markers at provided points.
        /// </summary>
        /// <param name="points">Points where Elevation Markers will be created.</param>
        /// <param name="viewFamilyType">ViewFamilyType to use.</param>
        /// <param name="scale">View scale for created Elevations.</param>
        /// <returns name="markers">Created Elevation Markers.</returns>
        /// <search>Revit.View.CreateElevationMarkers</search>
        [NodeCategory("Create")]
        public static List<DynElement?> CreateElevationMarkers(List<DynPoint> points,
            DynElement viewFamilyType, int scale = 50)
        {
            List<DynElement?> markers = new();

            if (points == null || points.Count == 0
                || viewFamilyType.InternalElement is not DB.ViewFamilyType dbVft
                || dbVft.ViewFamily != DB.ViewFamily.Elevation)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return Enumerable.Repeat<DynElement?>(null, points.Count).ToList();
            }

            DB.Document doc = dbVft.Document;

            doc.Ext_EnsureTransaction();

            foreach (DynPoint point in points)
            {
                var marker = DB.ElevationMarker.CreateElevationMarker(
                            doc, dbVft.Id, point.ToXyz(), scale);

                markers.Add(marker.Ext_ToDynElement(true));
            }

            doc.Ext_TransactionDone();

            return markers;
        }

        /// <summary>
        /// Creates Elevation Views from an Elevation Marker using provided indices.
        /// Existing Elevations at those indices will be returned if they already exist.
        /// </summary>
        /// <param name="elevationMarker">The Elevation Marker to create views from.</param>
        /// <param name="viewIndices">Indices of elevations to create.</param>
        /// <param name="view">Plan View used for the elevation context.</param>
        /// <returns name="views">Created or existing Elevation Views.</returns>
        /// <search>Revit.View.CreateElevations</search>
        [NodeCategory("Create")]
        public static List<DynElement?> CreateElevations(DynElement elevationMarker, List<int> viewIndices, DynView view)
        {
            List<DynElement?> elevations = new();

            if (elevationMarker.InternalElement is not DB.ElevationMarker dbMarker
                || view.InternalElement is not DB.View dbView
                || viewIndices == null
                || viewIndices.Count == 0)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return Enumerable.Repeat<DynElement?>(null, viewIndices.Count).ToList();
            }

            DB.Document doc = dbMarker.Document;

            doc.Ext_EnsureTransaction();

            foreach (int index in viewIndices)
            {
                DB.View? elevation = null;

                DB.ElementId existingId = dbMarker.GetViewId(index);

                // Return existing elevation if one already exists
                if (existingId != DB.ElementId.InvalidElementId)
                {
                    elevation = doc.GetElement(existingId) as DB.View;
                }
                // Otherwise attempt to create one
                else if (dbMarker.IsAvailableIndex(index))
                {
                    try
                    {
                        elevation = dbMarker.CreateElevation(
                            doc,
                            dbView.Id,
                            index);
                    }
                    catch
                    {
                        elevation = null;
                    }
                }

                elevations.Add(elevation?.Ext_ToDynElement(true));
            }

            doc.Ext_TransactionDone();

            return elevations;
        }

        /// <summary>
        /// Sets the Phase of provided Views.
        /// </summary>
        /// <param name="views">The Views to change the Phase of.</param>
        /// <param name="phase">The Phase to apply.</param>
        /// <returns name="success">Were the Views changed.</returns>
        /// <search>Revit.View.SetPhase</search>
        [NodeCategory("Action")]
        public static List<bool> SetPhase(List<DynView> views, DynElement phase)
        {
            // Ensure we were given a Revit phase
            DB.ElementId setPhaseId = DB.ElementId.InvalidElementId;
            if (phase.InternalElement is DB.Phase revitPhase) { setPhaseId = revitPhase.Id; }

            if (setPhaseId.Ext_IsInValid())
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Invalid phase provided.");
                return Enumerable.Repeat(false, views.Count).ToList();
            }
            
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<bool> success = new();

            // Transaction: Modify Views
            doc.Ext_EnsureTransaction();

            foreach (DynView view in views)
            {
                bool wasSet = false;
                
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
                            wasSet = true;
                        }
                        // Try to change phase
                        else
                        {
                            try
                            {
                                parameter.Set(setPhaseId);
                                wasSet = true;
                            }
                            catch {; }
                        }
                    }
                }

                success.Add(wasSet);
            }

            doc.Ext_TransactionDone();

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

            // Transaction: Set crop boundary
            DB.Document doc = dbView.Document;
            doc.Ext_EnsureTransaction();

            bool success = false;

            // Try to set crop box
            try
            {
                dbView.CropBoxActive = true;
                dbView.CropBoxVisible = visible;
                cropManager.SetCropShape(curveLoop);
                success = true;
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            doc.Ext_TransactionDone();

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
        /// <search>Revit.View.Ancestry</search>
        [NodeCategory("Query")]
        public static IList<DynElement?> Ancestry(DynView view, bool includeView = false)
        {
            if (view.InternalElement is DB.View dbView)
            {
                return dbView.Ext_GetAncestryViewIds()
                    .Ext_ToDynamoElements(true, dbView.Document);
            }
            return new List<DynElement?>();
        }

        /// <summary>
        /// Applies a View Template to provided Views.
        /// </summary>
        /// <param name="views">The Views to change.</param>
        /// <param name="viewTemplate">The View template to apply.</param>
        /// <returns name="success">Were the templates applied (or applied already).</returns>
        /// <search>Revit.View.SetViewTemplate</search>
        [NodeCategory("Action")]
        public static List<bool> SetViewTemplate(List<DynView> views, DynView viewTemplate)
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
                return Enumerable.Repeat(false, views.Count).ToList();
            }

            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<bool> success = new();

            // Transaction: Modify Views
            doc.Ext_EnsureTransaction();

            foreach (DynView view in views)
            {
                bool wasSet = false;

                if (view.InternalElement is DB.View dbView)
                {
                    // Already templated
                    if (dbView.ViewTemplateId == viewTemplateId)
                    {
                        wasSet = true;
                    }
                    else
                    {
                        // Try to set template
                        try
                        {
                            dbView.ViewTemplateId = viewTemplateId;
                            wasSet = true;
                        }
                        catch
                        {
                            wasSet = false;
                        }
                    }
                }

                success.Add(wasSet);
            }

            doc.Ext_TransactionDone();

            return success;
        }

        /// <summary>
        /// Removes the View Template from provided Views.
        /// </summary>
        /// <param name="views">The Views to change.</param>
        /// <returns name="success">Were the templates removed (or none already).</returns>
        /// <search>Revit.View.RemoveViewTemplate</search>
        [NodeCategory("Action")]
        public static List<bool> RemoveViewTemplate(List<DynView> views)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<bool> success = new();

            // Transaction: Modify Views
            doc.Ext_EnsureTransaction();

            foreach (DynView view in views)
            {
                bool wasSet = false;

                if (view.InternalElement is DB.View dbView)
                {
                    // Already no template
                    if (dbView.ViewTemplateId.Ext_IsInValid())
                    {
                        wasSet = true;
                    }
                    else
                    {
                        // Try to remove template
                        try
                        {
                            dbView.ViewTemplateId = DB.ElementId.InvalidElementId;
                            wasSet = true;
                        }
                        catch
                        {
                            wasSet = false;
                        }
                    }
                }

                success.Add(wasSet);
            }

            doc.Ext_TransactionDone();

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
                dbView.Document.Ext_EnsureAndCloseTransaction();

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
        /// <search>Revit.View.DependentViews</search>
        [NodeCategory("Query")]
        public static IEnumerable<DynElement> DependentViews(DynView view)
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
        /// <search>Revit.View.Plane</search>
        [NodeCategory("Query")]
        public static DynPlane? Plane(DynView view)
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
        /// <search>Revit.View.ViewTemplate</search>
        [NodeCategory("Query")]
        public static DynElement? ViewTemplate(DynView view)
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
        /// <search>Revit.View.Level</search>
        [NodeCategory("Query")]
        public static DynElement? Level(DynView view)
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
        /// <search>Revit.View.Phase</search>
        [NodeCategory("Query")]
        public static DynElement? Phase(DynView view)
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