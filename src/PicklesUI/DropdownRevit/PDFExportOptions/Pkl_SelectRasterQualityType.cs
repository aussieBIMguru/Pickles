using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectRasterQualityType")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Export")]
    [NodeDescription("Select from the available RasterQualityType options.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectRasterQualityType : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No formats found.";
        private const string OutputName = "settingName";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            return Enum.GetNames(typeof(DB.RasterQualityType)).ToList();
        }

        public Pkl_SelectRasterQualityType() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectRasterQualityType(
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