using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectExportPaperFormat")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Export")]
    [NodeDescription("Select from the available ExportPaperFormat options.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectExportPaperFormat : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No formats found.";
        private const string OutputName = "settingName";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            return Enum.GetNames(typeof(DB.ExportPaperFormat)).ToList();
        }

        public Pkl_SelectExportPaperFormat() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectExportPaperFormat(
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