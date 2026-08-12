using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectViewFamily")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_ViewFamilyType")]
    [NodeDescription("Select from the available ViewFamily options.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectViewFamily : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No options found.";
        private const string OutputName = "viewFamily";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            return Enum.GetNames(typeof(DB.ViewFamily)).ToList();
        }

        public Pkl_SelectViewFamily() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectViewFamily(
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