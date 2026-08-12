using Autodesk.Revit.DB;
using Revit.GeometryConversion;
namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Filled/Masking regions.
    /// </summary>
    public class Pkl_Region
    {
        internal Pkl_Region() { }

        /// <summary>
        /// Gets all GraphicStyles used by edges of a Region.
        /// </summary>
        /// <param name="region">The Region.</param>
        /// <returns name="styles">Styles used by the Region.</returns>
        /// <search>Revit.Region.GetEdgeStyles</search>
        [NodeCategory("Action")]
        public static List<DynElement?> GetEdgeStyles(DynElement region)
        {
            if (region.InternalElement is not DB.FilledRegion dbRegion)
            {
                return [];
            }

            var app = DocumentManager.Instance.CurrentUIApplication.Application;
            var options = app.Create.NewGeometryOptions();

            DB.Solid? solid = dbRegion
                .get_Geometry(options)
                .OfType<DB.Solid>()
                .FirstOrDefault();

            if (solid == null)
            {
                return [];
            }

            Document doc = dbRegion.Document;
            List<DynElement?> styles = [];

            foreach (Edge edge in solid.Edges)
            {
                styles.Add(edge.GraphicsStyleId.Ext_GetDynamoElement(doc, true));
            }

            return styles.Distinct().ToList();
        }

        /// <summary>
        /// Gets the boundary curves of a Region.
        /// </summary>
        /// <param name="region">The Region.</param>
        /// <returns name="curves">Boundary curves used by the Region.</returns>
        /// <search>Revit.Region.BoundaryCurves</search>
        [NodeCategory("Query")]
        public static List<List<DynCurve>> BoundaryCurves(DynElement region)
        {
            if (region.InternalElement is not DB.FilledRegion dbRegion)
            {
                return [];
            }

            return dbRegion
                .GetBoundaries()
                .Select(loop => loop
                    .Select(c => c.ToProtoType())
                    .ToList())
                .ToList();
        }

        /// <summary>
        /// Gets whether a Region is a masking region.
        /// </summary>
        /// <param name="region">The Region.</param>
        /// <returns name="isMasking">Whether the Region is masking.</returns>
        /// <search>Revit.Region.IsMaskingRegionType</search>
        [NodeCategory("Query")]
        public static bool IsMaskingRegion(DynElement region)
        {
            if (region.InternalElement is not DB.FilledRegion dbRegion)
            {
                return false;
            }

            return dbRegion.Document
                .GetElement(dbRegion.GetTypeId()) is DB.FilledRegionType regionType
                && regionType.IsMasking;
        }
    }
}
