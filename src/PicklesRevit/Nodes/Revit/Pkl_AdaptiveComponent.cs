using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to AdaptiveComponents.
    /// </summary>
    public class Pkl_AdaptiveComponent
    {
        internal Pkl_AdaptiveComponent() { }

        /// <summary>
        /// Places an Adaptive Component family by points.
        /// </summary>
        /// <param name="familyType">FamilyType to use.</param>
        /// <param name="points">Points for Adaptive Component to use.</param>
        /// <returns name="familyInstance">The created FamilyInstance.</returns>
        /// <search>Revit.AdaptiveComponent.CreateByPoints</search>
        [NodeCategory("Create")]
        public static DynElement? CreateByPoints(DynFamilySymbol familyType, List<DynPoint> points)
        {
            // Ensure Family type is valid for Adaptive Component use
            if (familyType.InternalElement is not DB.FamilySymbol familySymbol
                || !DB.AdaptiveComponentFamilyUtils.IsAdaptiveComponentFamily(familySymbol.Family)
                || points == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            DB.Document doc = familySymbol.Document;

            TransactionManager.Instance.ForceCloseTransaction();

            using (var t = new DB.Transaction(doc, "Pickles.FamilyInstance.CreateAdaptiveComponent"))
            {
                t.Start();

                // Activate the FamilyType
                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                // Place the Adaptive Component
                DB.FamilyInstance familyInstance =
                    DB.AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(
                        doc,
                        familySymbol);

                // Get the references for the points of the Component
                IList<DB.ElementId> pointIds =
                    DB.AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(
                        familyInstance);

                // Catch and rollback if the points do not match those provided in count
                if (pointIds.Count != points.Count)
                {
                    WARNING_TYPE.DEFAULT.Ext_Raise(
                        "Point count does not match the Adaptive Point count in the provided FamilyType.");

                    t.Ext_RollbackIfOpen();
                    return null;
                }

                // Set the points
                for (int i = 0; i < pointIds.Count; i++)
                {
                    // Catch if a point is invalid, rollback the change
                    if (doc.GetElement(pointIds[i]) is not DB.ReferencePoint point)
                    {
                        WARNING_TYPE.DEFAULT.Ext_Raise(
                            "Failed to retrieve Adaptive Component point.");

                        t.Ext_RollbackIfOpen();
                        return null;
                    }

                    point.Position = points[i].ToXyz();
                }

                t.Ext_CommitIfOpen();

                // Return the family
                return familyInstance.Ext_ToDynElement(true);
            }
        }

        /// <summary>
        /// Updates an Adaptive Component family to new points.
        /// </summary>
        /// <param name="adaptiveComponent">FamilyType to use.</param>
        /// <param name="points">Points for Adaptive Component to use.</param>
        /// <returns name="success">Was the update successful.</returns>
        /// <search>Revit.AdaptiveComponent.SetPoints</search>
        [NodeCategory("Action")]
        public static bool SetPoints(DynAdaptiveComponent adaptiveComponent, List<DynPoint> points)
        {
            // Ensure Family type is valid for Adaptive Component use
            if (adaptiveComponent.InternalElement is not DB.FamilyInstance familyInstance
                || !DB.AdaptiveComponentFamilyUtils.IsAdaptiveComponentFamily(familyInstance.Symbol.Family)
                || points == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return false;
            }

            DB.Document doc = familyInstance.Document;

            TransactionManager.Instance.ForceCloseTransaction();

            using (var t = new DB.Transaction(doc, "Pickles.FamilyInstance.UpdateAdaptiveComponent"))
            {
                t.Start();

                // Get the references for the points of the Component
                IList<DB.ElementId> pointIds =
                    DB.AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(
                        familyInstance);

                // Catch and rollback if the points do not match those provided in count
                if (pointIds.Count != points.Count)
                {
                    WARNING_TYPE.DEFAULT.Ext_Raise(
                        "Point count does not match the Adaptive Point count in the provided FamilyType.");

                    t.Ext_RollbackIfOpen();
                    return false;
                }

                // Set the points
                for (int i = 0; i < pointIds.Count; i++)
                {
                    // Catch if a point is invalid, rollback the change
                    if (doc.GetElement(pointIds[i]) is not DB.ReferencePoint point)
                    {
                        WARNING_TYPE.DEFAULT.Ext_Raise(
                            "Failed to retrieve Adaptive Component point.");

                        t.Ext_RollbackIfOpen();
                        return false;
                    }

                    point.Position = points[i].ToXyz();
                }

                t.Ext_CommitIfOpen();
            }

            // Return success
            return true;
        }

        /// <summary>
        /// Returns the Points in an AdaptiveComponent.
        /// </summary>
        /// <param name="adaptiveComponent">FamilyInstance to query.</param>
        /// <returns name="points">The related Points, if any.</returns>
        /// <search>Revit.AdaptiveComponent.GetPoints</search>
        [NodeCategory("Action")]
        public static List<DynPoint> GetPoints(DynAdaptiveComponent adaptiveComponent)
        {
            // Ensure Family type is valid for Adaptive Component use
            if (adaptiveComponent.InternalElement is not DB.FamilyInstance familyInstance
                || !DB.AdaptiveComponentFamilyUtils.IsAdaptiveComponentFamily(familyInstance.Symbol.Family))
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return new();
            }

            DB.Document doc = familyInstance.Document;

            return DB.AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(familyInstance)
                .Select(i => i.Ext_GetElement<DB.ReferencePoint>(doc))
                .Where(r => r != null)
                .Select(r => r.Position.ToPoint())
                .ToList();
        }
    }
}