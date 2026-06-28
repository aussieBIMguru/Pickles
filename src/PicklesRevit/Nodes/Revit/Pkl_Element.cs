namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Elements.
    /// </summary>
    public class Pkl_Element
    {
        internal Pkl_Element() { }

        /// <summary>
        /// Attempt to delete provided Elements.
        /// </summary>
        /// <param name="elements">Elements to delete.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="success">Was the Element deleted successfully.</returns>
        /// <search>Revit.Element.Delete</search>
        [NodeCategory("Create")]
        public static List<bool> Delete(List<DynElement> elements,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance);
            List<bool> success = new();

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return success;
            }

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();
            DB.Document doc = docHelper.Document;

            // Using a transaction...
            using (var transaction = new DB.Transaction(doc, "Pickle: Elements.Delete"))
            {
                transaction.Start();

                // Try to delete each element
                foreach (DynElement element in elements)
                {
                    try
                    {
                        doc.Delete(element.InternalElement.Id);
                        success.Add(true);
                    }
                    catch
                    {
                        success.Add(false);
                    }
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Return output
            return success;
        }

        /// <summary>
        /// Populates the bounded volume of an Element with random points.
        /// </summary>
        /// <param name="element">Element to populate.</param>
        /// <param name="count">Number of points.</param>
        /// <param name="seed">Randomization seed.</param>
        /// <returns name="points">Populated points.</returns>
        /// <search>Revit.Element.Populate</search>
        [NodeCategory("Create")]
        public static List<DynPoint> Populate(DynElement element, int count = 1, int seed = 1)
        {
            // Points to return
            List<DynPoint> points = new();

            // Assess Bb size and min/max
            DynBb bb = element.BoundingBox;
            DynPoint bbMin = bb.MinPoint;
            DynPoint bbMax = bb.MaxPoint;
            double width = bbMax.X - bbMin.X;
            double depth = bbMax.Y - bbMin.Y;
            double height = bbMax.Z - bbMin.Z;

            // Randomized seed
            Random rand = new Random(seed);

            // Populate each point
            for (int i = 0; i < count; i++)
            {
                double x = bbMin.X + rand.NextDouble() * width;
                double y = bbMin.Y + rand.NextDouble() * depth;
                double z = bbMin.Z + rand.NextDouble() * height;
                points.Add(DynPoint.ByCoordinates(x, y, z));
            }

            // Return output
            return points;
        }

        /// <summary>
        /// Gets Elements by provided integer or string.
        /// </summary>
        /// <param name="elementIds">Integers, strings or DB.ElementIds.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="elements">Elements of the provided Ids.</returns>
        /// <search>Revit.Element.GetByIdInDocument</search>
        [NodeCategory("Action")]
        public static List<DynElement> GetByIdInDocument(List<object> elementIds,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance);
            List<DynElement> dynElements = new();

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return dynElements;
            }

            DB.Document doc = docHelper.Document;

            // For each anonymous Id...
            foreach (object elementId in elementIds)
            {
                string idString = elementId.ToString();
                if (elementId is DB.ElementId id) { idString = id.Value.ToString(); }
                int idWithFallback = idString.Ext_ToIntWithFallback(-1);
                DB.ElementId revitId = new DB.ElementId(idWithFallback);
                dynElements.Add(revitId.Ext_GetDynamoElement(doc, true));
            }

            // Return elements
            return dynElements;
        }

        /// <summary>
        /// Gets Element by provided Id string or integer.
        /// </summary>
        /// <param name="elementId">Id string or integer.</param>
        /// <returns name="element">Element of the provided Id.</returns>
        /// <search>Revit.Element.GetById</search>
        [NodeCategory("Action")]
        public static DynElement? GetById(object elementId)
        {
            int idWithFallback = elementId.ToString().Ext_ToIntWithFallback(-1);
            return Revit.Elements.ElementSelector.ByElementId(idWithFallback);
        }

        /// <summary>
        /// Gets Element by provided Guid string.
        /// </summary>
        /// <param name="elementGuid">Guid string.</param>
        /// <returns name="element">Element of the provided Guid.</returns>
        /// <search>Revit.Element.GetByGuid</search>
        [NodeCategory("Action")]
        public static DynElement GetByGuid(string elementGuid)
        {
            return Revit.Elements.ElementSelector.ByUniqueId(elementGuid);
        }

        /// <summary>
        /// Gets the Element's type if it has one.
        /// </summary>
        /// <param name="element">The Element.</param>
        /// <returns name="type">The Element's type, if it has one.</returns>
        /// <search>Revit.Element.GetType</search>
        [NodeCategory("Action")]
        public static DynElement? GetType(DynElement element)
        {
            return element.InternalElement?
                .Ext_GetType()
                .Ext_ToDynElement(true);
        }

        /// <summary>
        /// Gets the owner View of an Element if it is view specific.
        /// </summary>
        /// <param name="element">The Element.</param>
        /// <returns name="view">The Element's owner View, if it has one.</returns>
        /// <search>Revit.Element.GetOwnerView</search>
        [NodeCategory("Action")]
        public static DynElement? GetOwnerView(DynElement element)
        {
            return element.InternalElement?
                .OwnerViewId.Ext_GetDynamoElement(element.InternalElement.Document, true);
        }

        /// <summary>
        /// Returns the Workset an Element is on.
        /// </summary>
        /// <param name="element">The Element to query.</param>
        /// <returns name="workset">The Workset of the Element.</returns>
        /// <search>Revit.Element.GetWorkset</search>
        [NodeCategory("Action")]
        public static DB.Workset? GetWorkset(DynElement element)
        {
            // Get the current document
            DB.Element revitElement = element.InternalElement;
            DB.Document doc = revitElement.Document;

            // Early return if document is not workshared
            if (!doc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
                return null;
            }

            return doc.GetWorksetTable().GetWorkset(revitElement.WorksetId);
        }

        /// <summary>
        /// Returns the Group an Element is in, if any.
        /// </summary>
        /// <param name="element">The Element to query.</param>
        /// <returns name="group">The Group of the Element, if any.</returns>
        /// <search>Revit.Element.GetGroup</search>
        [NodeCategory("Action")]
        public static DynElement? GetGroup(DynElement element)
        {
            // Get the current document
            DB.Element revitElement = element.InternalElement;
            DB.Document doc = revitElement.Document;
            return revitElement.GroupId.Ext_GetDynamoElement(doc, true);
        }

        /// <summary>
        /// Sets all Elements to a given Workset.
        /// </summary>
        /// <param name="elements">The Element to set.</param>
        /// <param name="workset">The Workset to apply.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to transact in (current if not provided).</param>
        /// <returns name="success">Did the change succeed.</returns>
        /// <search>Revit.Element.SetWorkset</search>
        [NodeCategory("Action")]
        public static List<bool> SetWorkset(List<DynElement> elements, DB.Workset workset,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance);
            List<bool> success = new();

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return success;
            }

            DB.Document doc = docHelper.Document;

            // Early return if document is not workshared
            if (!doc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
            }

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(doc, "Pickle: Elements.SetWorkset"))
            {
                transaction.Start();

                int wsId = workset.Id.IntegerValue;

                // Try to set the Workset
                foreach (DynElement element in elements)
                {
                    try
                    {
                        DB.Parameter parameter = element.InternalElement
                            .get_Parameter(DB.BuiltInParameter.ELEM_PARTITION_PARAM);
                        parameter.Set(wsId);
                        success.Add(true);
                    }
                    catch
                    {
                        success.Add(false);
                    }
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Return output
            return success;
        }

        /// <summary>
        /// Attempt to rename provided Elements.
        /// </summary>
        /// <param name="elements">Elements to rename.</param>
        /// <param name="names">Names to apply.</param>
        /// <param name="parameterName">Parameter name to set.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="elements">The Elements.</returns>
        /// <returns name="success">Was the Element deleted successfully.</returns>
        /// <search>Revit.Element.Rename</search>
        [NodeCategory("Action")]
        [MultiReturn("elements", "success")]
        public static Dictionary<string, object> Rename(List<DynElement> elements, List<string> names,
            string parameterName = "Name", [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, true);

            // Output dictionary
            var elementsOut = new List<DynElement>();
            var success = new List<bool>();

            var output = new Dictionary<string, object>
            {
                { "elements", elements },
                { "success", success }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Unequal length warning (proceed with shortest)
            if (elements.Count != names.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
            }

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();
            DB.Document doc = docHelper.Document;

            // Using a transaction...
            using (var transaction = new DB.Transaction(doc, "Pickle: Elements.Rename"))
            {
                transaction.Start();

                // Rename each element
                for (int i = 0; i < Math.Min(elements.Count, names.Count); i++)
                {
                    DB.Element revitElement = elements[i].InternalElement;
                    DB.Parameter parameter = revitElement.LookupParameter(parameterName);

                    elementsOut.Add(elements[i]);

                    try
                    {
                        parameter.Set(names[i]);
                        success.Add(true);
                    }
                    catch
                    {
                        success.Add(false);
                    }
                }

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Return output
            return output;
        }

        /// <summary>
        /// Isolates a list of Elements in a given View.
        /// </summary>
        /// <param name="elements">Elements to isolate.</param>
        /// <param name="view">View to isolate Elements in.</param>
        /// <returns name="success">Did the isolation work.</returns>
        /// <search>Revit.Element.IsolateInView</search>
        [NodeCategory("Action")]
        public static bool IsolateInView(List<DynElement> elements, DynView view)
        {
            DB.View revitView = view.Ext_ToRevitView();
            DB.Document doc = revitView?.Document;

            if (revitView is null || doc is null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return false;
            }
            
            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(doc, "Pickle: Elements.IsolateInView"))
            {
                transaction.Start();

                // Get Element Id list
                List<DB.ElementId> ids = elements
                    .Select(e => e.InternalElement.Id)
                    .ToList();

                // Isolate elements
                revitView.IsolateElementsTemporary(ids);

                transaction.Commit();
            }

            TransactionManager.Instance.TransactionTaskDone();

            // Return output
            return true;
        }

        /// <summary>
        /// Selects provided Elements in Revit.
        /// </summary>
        /// <param name="elements">Elements to select.</param>
        /// <returns name="success">Did the selection work.</returns>
        /// <search>Revit.Element.Select</search>
        [NodeCategory("Action")]
        public static bool Select (List<DynElement> elements)
        {
            // Get active UI Document
            RUI.UIDocument uiDoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;

            // Ids to select
            List<DB.ElementId> selectIds = elements
                .Select(e => e?.InternalElement?.Id)
                .Where(e => e != null)
                .Distinct()
                .ToList();

            // Try to select
            try
            {
                uiDoc.Selection.SetElementIds(selectIds);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an Element is editable by you.
        /// </summary>
        /// <param name="element">The Element to check.</param>
        /// <returns name="editable">If the Element is editable by you.</returns>
        /// <search>Revit.Element.IsEditable</search>
        [NodeCategory("Query")]
        public static bool IsEditable(DynElement element)
        {
            DB.Element revitElement = element.InternalElement;
            DB.Document doc = revitElement.Document;

            // Early return if document is not workshared
            if (!doc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
                return true;
            }

            DB.CheckoutStatus checkoutStatus = DB.WorksharingUtils.GetCheckoutStatus(doc, revitElement.Id);
            DB.ModelUpdatesStatus updateStatus = DB.WorksharingUtils.GetModelUpdatesStatus(doc, revitElement.Id);

            if (checkoutStatus == DB.CheckoutStatus.OwnedByOtherUser) return false;
            if (checkoutStatus == DB.CheckoutStatus.OwnedByCurrentUser) return true;
            return updateStatus == DB.ModelUpdatesStatus.CurrentWithCentral;
        }

        /// <summary>
        /// Returns the WorksharingTooltipInfo properties of an Element.
        /// </summary>
        /// <param name="element">The Element to check.</param>
        /// <returns name="createdBy">Who created the Element.</returns>
        /// <returns name="ownedBy">Who owns the Element.</returns>
        /// <returns name="lastChangedBy">Who last changed the Element.</returns>
        /// <search>Revit.Element.Owners</search>
        [NodeCategory("Query")]
        [MultiReturn("createdBy", "ownedBy", "lastChangedBy")]
        public static Dictionary<string, object> Owners(DynElement element)
        {
            DB.Element revitElement = element.InternalElement;
            DB.Document doc = revitElement.Document;
            
            // Early return if document is not workshared
            if (!doc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
            }

            // Return the owner properties of tooltip info
            DB.WorksharingTooltipInfo tti = DB.WorksharingUtils.GetWorksharingTooltipInfo(doc, revitElement.Id);

            return new Dictionary<string, object>()
            {
                { "createdBy", tti.Creator },
                { "ownedBy", tti.Owner },
                { "lastChangedBy", tti.LastChangedBy }
            };
        }

        /// <summary>
        /// Returns the Worksharing CheckoutStatus of an Element
        /// </summary>
        /// <param name="element">The Element to query.</param>
        /// <returns name="checkoutStatus">The status of the Element.</returns>
        /// <search>Revit.Element.CheckoutStatus</search>
        [NodeCategory("Query")]
        public static string CheckoutStatus(DynElement element)
        {
            DB.Element revitElement = element.InternalElement;
            DB.Document doc = revitElement.Document;
            return DB.WorksharingUtils.GetCheckoutStatus(doc, revitElement.Id).ToString();
        }

        /// <summary>
        /// Returns the Worksharing ModelUpdateStatus of an Element
        /// </summary>
        /// <param name="element">The Element to query.</param>
        /// <returns name="checkoutStatus">The status of the Element.</returns>
        /// <search>Revit.Element.ModelUpdateStatus</search>
        [NodeCategory("Query")]
        public static string ModelUpdateStatus(DynElement element)
        {
            DB.Element revitElement = element.InternalElement;
            DB.Document doc = revitElement.Document;
            return DB.WorksharingUtils.GetModelUpdatesStatus(doc, revitElement.Id).ToString();
        }

        /// <summary>
        /// Gets the centroid point of an Element's bounding box.
        /// </summary>
        /// <param name="element">The Element to get the centroid point of.</param>
        /// <returns name="point">The Element's centroid point.</returns>
        /// <search>Revit.Element.Centroid</search>
        [NodeCategory("Query")]
        public static DynPoint Centroid(DynElement element)
        {
            DynBb bb = element.BoundingBox;

            DynPoint bbMax = bb.MinPoint;
            DynPoint bbMin = bb.MaxPoint;

            return DynPoint.ByCoordinates((bbMin.X + bbMax.X) / 2,
                (bbMin.Y + bbMax.Y) / 2,
                (bbMin.Z + bbMax.Z) / 2);
        }

        /// <summary>
        /// Attempts to get the value of the parameter of an Element at either an instance or type level.
        /// </summary>
        /// <param name="element">The Element to get the value for.</param>
        /// <param name="parameterName">The name of the Parameter.</param>
        /// <returns name="value">The Parmaeter value if found.</returns>
        /// <search>Revit.Element.GetParameterValueByName</search>
        [NodeCategory("Action")]
        public static object? GetParameterValueByName(DynElement element, string parameterName)
        {
            DB.Element revitElement = element.InternalElement;
            DB.Element? revitType = revitElement.Ext_GetType();

            DB.Parameter? parameter = revitElement.LookupParameter(parameterName)
                                  ?? revitType?.LookupParameter(parameterName);

            return parameter?.Ext_GetParameterValueAsObject(revitElement.Document);
        }
    }
}