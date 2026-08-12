using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to RevitLinkInstances.
    /// </summary>
    public class Pkl_RevitLinkInstance
    {
        internal Pkl_RevitLinkInstance() { }

        /// <summary>
        /// Gets the underlying Transform applied to the RevitLinkInstance.
        /// </summary>
        /// <param name="revitLinkInstance">Instance to get the transform of.</param>
        /// <returns name="coordinateSystem">The transform as a Dynamo CoordinateSystem.</returns>
        /// <search>Revit.RevitLinkInstance.GetTransform</search>
        [NodeCategory("Action")]
        public static DynCoordinateSystem? GetTransform(DynElement revitLinkInstance)
        {
            if (revitLinkInstance.InternalElement is DB.RevitLinkInstance dbLinkInstance)
            {
                return dbLinkInstance.GetTotalTransform().ToCoordinateSystem();
            }
            return null;
        }

        /// <summary>
        /// Gets the RevitLinkType of the RevitLinkInstance.
        /// </summary>
        /// <param name="revitLinkInstance">Instance to get the RevitLinkType of.</param>
        /// <returns name="revitLinkType">The RevitLinkType of the instance.</returns>
        /// <search>Revit.RevitLinkInstance.GetRevitLinkType</search>
        [NodeCategory("Action")]
        public static DynElement? GetRevitLinkType(DynElement revitLinkInstance)
        {
            if (revitLinkInstance.InternalElement is DB.RevitLinkInstance dbLinkInstance)
            {
                return dbLinkInstance.Ext_GetType()?.Ext_ToDynElement(true);
            }
            return null;
        }
    }
}