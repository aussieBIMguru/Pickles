using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectViewSheetSet")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_ViewSheetSet")]
    [NodeDescription("Select a ViewSheetSet from the current document.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectViewSheetSet : DropDownFactoryBase<DB.ViewSheetSet>
    {
        private const string NoItems = "No sets available in project.";
        private const string OutputName = "viewSheetSet";

        private static IEnumerable<DB.ViewSheetSet> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return [];

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.ViewSheetSet))
                .Cast<DB.ViewSheetSet>();
        }


        public Pkl_SelectViewSheetSet() : base(
            OutputName, NoItems,
            GetItems,
            s => s.Name,
            new ElementOutputStrategy<DB.ViewSheetSet>())
        { }

        [JsonConstructor]
        public Pkl_SelectViewSheetSet(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            s => s.Name,
            new ElementOutputStrategy<DB.ViewSheetSet>(),
            inPorts, outPorts)
        { }
    }
}