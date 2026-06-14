using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace PicklesUI
{
    /// <summary>
    /// Select from pickled keys.
    /// </summary>
    [NodeName("Pkl_SelectByKey1")]
    [NodeCategory("Pickles.Pkl_Data.Pkl_Pickling")]
    [NodeDescription("Select from a list of pickled objects, which can then be unpickled in your graph.\n\n" +
        "This node draws upon the first pickled key in the graph.\n\n" +
        "NOTE: Works in canvas and player.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectByKey1 : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No pickles found.";
        private const string OutputName = "pickles";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            var keyName = GraphStorage.GetKeys().FirstOrDefault() ?? "";

            if (!GraphStorage.TryGet(keyName, out var value))
            {
                return new List<string>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(value)
                    ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public Pkl_SelectByKey1()
            : base(
                OutputName,
                NoItems,
                GetItems,
                s => s.Ext_ExtractPickleKey(),
                new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectByKey1(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts)
            : base(
                OutputName,
                NoItems,
                GetItems,
                s => s.Ext_ExtractPickleKey(),
                new StringOutputStrategy(),
                inPorts,
                outPorts)
        { }
    }
}