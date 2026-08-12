using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectPdfExportQualityType")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Export")]
    [NodeDescription("Select from the available PdfExportQualityType options.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectPdfExportQualityType : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No formats found.";
        private const string OutputName = "settingName";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            return Enum.GetNames(typeof(DB.PDFExportQualityType)).ToList();
        }

        public Pkl_SelectPdfExportQualityType() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectPdfExportQualityType(
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