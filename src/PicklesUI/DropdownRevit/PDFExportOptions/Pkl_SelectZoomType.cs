using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectZoomType")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Export")]
    [NodeDescription("Select from the available ZoomType options.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectZoomType : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No formats found.";
        private const string OutputName = "settingName";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            return Enum.GetNames(typeof(DB.ZoomType)).ToList();
        }

        public Pkl_SelectZoomType() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectZoomType(
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