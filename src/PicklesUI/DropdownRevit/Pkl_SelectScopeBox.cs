using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectScopeBox")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_ScopeBox")]
    [NodeDescription("Select from the available ScopeBoxes.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectScopeBox : DropDownFactoryBaseCore<DB.Element>
    {
        private const string NoItems = "No ScopeBox found.";
        private const string OutputName = "scopeBox";

        private static IEnumerable<DB.Element> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return [];

            return new DB.FilteredElementCollector(doc)
                .OfCategory(DB.BuiltInCategory.OST_VolumeOfInterest);
        }

        public Pkl_SelectScopeBox() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x.Name,
            new ElementOutputStrategy<DB.Element>())
        { }

        [JsonConstructor]
        public Pkl_SelectScopeBox(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName,
            NoItems,
            GetItems,
            x => x.Name,
            new ElementOutputStrategy<DB.Element>(),
            inPorts, outPorts)
        { }
    }
}