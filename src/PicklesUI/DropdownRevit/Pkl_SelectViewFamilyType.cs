using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectViewFamilyType")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Selection")]
    [NodeDescription("Select a view family type from the current document.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectViewFamilyType : DropDownFactoryBase<DB.ViewFamilyType>
    {
        private const string NoItems = "No view family types available in project.";
        private const string OutputName = "viewFamilyType";

        private static IEnumerable<DB.ViewFamilyType> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return Enumerable.Empty<DB.ViewFamilyType>();

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.ViewFamilyType))
                .Cast<DB.ViewFamilyType>();
        }

        public Pkl_SelectViewFamilyType() : base(
            OutputName, NoItems,
            GetItems,
            x => $"{x.ViewFamily}: {x.Name}",
            new ElementOutputStrategy<DB.ViewFamilyType>())
        { }

        [JsonConstructor]
        public Pkl_SelectViewFamilyType(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            x => $"{x.ViewFamily}: {x.Name}",
            new ElementOutputStrategy<DB.ViewFamilyType>(),
            inPorts, outPorts)
        { }
    }
}