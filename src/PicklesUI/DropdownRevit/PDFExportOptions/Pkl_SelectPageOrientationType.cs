using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectPageOrientationType")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Export")]
    [NodeDescription("Select from the available PageOrientationType options.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectPageOrientationType : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No formats found.";
        private const string OutputName = "settingName";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            return Enum.GetNames(typeof(DB.PageOrientationType)).ToList();
        }

        public Pkl_SelectPageOrientationType() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectPageOrientationType(
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