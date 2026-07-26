using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectBoundaryLocation")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Room")]
    [NodeDescription("Select from the available SpatialElementBoundaryLocation options.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectBoundaryLocation : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No options found.";
        private const string OutputName = "boundaryLocation";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            return Enum.GetNames(typeof(DB.SpatialElementBoundaryLocation)).ToList();
        }

        public Pkl_SelectBoundaryLocation() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectBoundaryLocation(
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