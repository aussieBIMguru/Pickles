using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectPrintSetting")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Export")]
    [NodeDescription("Select from the available Revit print settings.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectPrintSetting: DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No settings found.";
        private const string OutputName = "printSetting";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.PrintSetting))
                .Select(s => s.Name)
                .OrderBy(n => n)
                .ToList();
        }

        public Pkl_SelectPrintSetting() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectPrintSetting(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy(),
            inPorts,
            outPorts)
        { }
    }
}