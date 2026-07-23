using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to the Spaces.
    /// </summary>
    public class Pkl_Space
    {
        internal Pkl_Space() { }

        /// <summary>
        /// Gets the Space at given Points, if any.
        /// 
        /// If a RevitLinkInstance is provided, the transform will be accounted for by the node.
        /// </summary>
        /// <param name="points">The Points to query.</param>
        /// <param name="phase">The Phase to query (checks all in order if not provided).</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="space">The Spaces at the Points, if any.</returns>
        /// <search>Revit.Space.GetAtPoint</search>
        [NodeCategory("Action")]
        public static List<DynElement?> GetAtPoint(List<DynPoint> points,
            [DefaultArgument("null")] DynElement? phase = null,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return Enumerable.Repeat<DynElement?>(null, points.Count).ToList();
            }

            DB.Document doc = docHelper.Document;
            List<DynElement?> spaces = new();

            // Find the specified Phase (regardless of in same Document), all Phases if not found
            string phaseName = phase == null ? string.Empty : phase.Name;
            List<DB.Phase> phaseList = new List<DB.Phase>();

            foreach (DB.Phase dbPhase in doc.Phases)
            {
                if (dbPhase.Name == phaseName)
                {
                    phaseList = new() { dbPhase };
                    break;
                }
                phaseList.Add(dbPhase);
            }

            // Get the Link transform if one was provided
            DB.Transform transform = null;

            if (docOrLinkInstance is DynElement dynElement
                && dynElement.InternalElement is DB.RevitLinkInstance linkInstance)
            {
                transform = linkInstance.GetTotalTransform().Inverse;
            }

            bool isTransformed = transform != null;

            // Get the Space at each point
            foreach (DynPoint point in points)
            {
                DB.XYZ dbPoint = isTransformed
                    ? transform.OfPoint(point.ToXyz())
                    : point.ToXyz();

                DbSpace space = null;

                foreach (DB.Phase checkPhase in phaseList)
                {
                    space = doc.GetSpaceAtPoint(dbPoint, checkPhase);
                    if (space != null) { break; }
                }

                spaces.Add(space?.Ext_ToDynElement(true));
            }

            return spaces;
        }

        /// <summary>
        /// Gets the SpaceType of a Space.
        /// </summary>
        /// <param name="space">The Space to query.</param>
        /// <returns name="SpaceType">The Space's SpaceType.</returns>
        /// <search>Revit.Space.SpaceType</search>
        [NodeCategory("Query")]
        public static DynElement? SpaceType(DynElement space)
        {
            if (space.InternalElement is DbSpace dbSpace)
            {
                return dbSpace.SpaceTypeId
                    .Ext_GetDynamoElement(dbSpace.Document, true);
            }
            return null;
        }
    }
}