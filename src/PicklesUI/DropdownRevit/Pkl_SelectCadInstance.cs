using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectCadInstance")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Selection")]
    [NodeDescription("Select a CAD instance from the current document.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectCadInstance : DropDownFactoryBase<DB.ImportInstance>
    {
        private const string NoItems = "No CAD Links available in project.";
        private const string OutputName = "importInstance";

        private static IEnumerable<DB.ImportInstance> GetItems()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return Enumerable.Empty<DB.ImportInstance>();

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.ImportInstance))
                .Cast<DB.ImportInstance>();
        }


        public Pkl_SelectCadInstance() : base(
            OutputName, NoItems,
            GetItems,
            x => $"{x.Name} ({x.Id})",
            new ElementOutputStrategy<DB.ImportInstance>())
        { }

        [JsonConstructor]
        public Pkl_SelectCadInstance(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            x => $"{x.Name} ({x.Id})",
            new ElementOutputStrategy<DB.ImportInstance>(),
            inPorts, outPorts)
        { }
    }
}