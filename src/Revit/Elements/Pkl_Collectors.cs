// Autodesk
using Autodesk.DesignScript.Runtime;
using DynamoServices;
using DB = Autodesk.Revit.DB;
// Pickle
using pklGen = Pkl_Utilities.Pkl_General;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to collection of elements.
    /// </summary>
    public class Pkl_Collectors
    {
        internal Pkl_Collectors() { }

        /// <summary>
        /// Collects all ceilings in a linked instance's document or the current document if not provided.
        /// </summary>
        /// <param name="docOrLinkInstance ">Optional Revit linked instance to get ceilings from.</param>
        /// <returns>A list of ceilings.</returns>
        /// <search>collect, ceiling</search>
        [MultiReturn("ceilings")]
        public static Dictionary<string, object> GetAllCeilings([DefaultArgument("null")] global::Revit.Elements.Element? docOrLinkInstance = null)
        {
            DB.Document? doc = pklGen.GetDocumentFromLinkedInstance(docOrLinkInstance, fallBack: true);
            string outputName1 = "ceilings";

            var output = new Dictionary<string, object>
            {
                { outputName1, new List<global::Revit.Elements.Element>() }
            };

            if (doc is null)
            {
                LogWarningMessageEvents.OnLogWarningMessage("Linked document could not be retrieved. Is the link loaded?");
                return output;
            }

            output[outputName1] = pklGen.DynElementsByRvtCategory(DB.BuiltInCategory.OST_Ceilings, doc);
            return output;
        }

        /// <summary>
        /// Collects all doors in a linked instance's document or the current document if not provided.
        /// </summary>
        /// <param name="docOrLinkInstance ">Optional Revit linked instance to get doors from.</param>
        /// <returns>A list of doors.</returns>
        /// <search>collect, door</search>
        [MultiReturn("doors")]
        public static Dictionary<string, object> GetAllDoors([DefaultArgument("null")] global::Revit.Elements.Element? docOrLinkInstance = null)
        {
            DB.Document? doc = pklGen.GetDocumentFromLinkedInstance(docOrLinkInstance, fallBack: true);
            string outputName1 = "doors";

            var output = new Dictionary<string, object>
            {
                { outputName1, new List<global::Revit.Elements.Element>() }
            };

            if (doc is null)
            {
                LogWarningMessageEvents.OnLogWarningMessage("Linked document could not be retrieved. Is the link loaded?");
                return output;
            }

            output[outputName1] = pklGen.DynElementsByRvtCategory(DB.BuiltInCategory.OST_Doors, doc);
            return output;
        }
    }
}