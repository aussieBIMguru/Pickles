using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace PicklesUI
{

    /// <summary>
    /// Select from pickled keys.
    /// </summary>
    [NodeName("Pkl_SelectByNodeName")]
    [NodeCategory("Pickles.Pkl_Data.Pkl_Pickling")]
    [NodeDescription("Select from a list of pickled objects, which can then be unpickled in your graph.\n\n" +
        "Rename the node to match a pickled key to access its values (otherwise it uses the first key, if available).\n\n" +
        "NOTE: Only works on canvas, not in player.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectByNodeName : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No pickles found.";
        private const string OutputName = "pickles";

        /// <summary>
        /// Do not allow to be an input node (doesn't work in player).
        /// </summary>
        public override bool IsInputNode => false;

        [JsonProperty]
        public string PickleKey { get; set; } = "";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            var n = (Pkl_SelectByNodeName)node;
            var keyName = n.PickleKey;

            if (!GraphStorage.Contains(keyName))
            {
                keyName = GraphStorage.GetKeys().FirstOrDefault() ?? "";
            }

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

        public Pkl_SelectByNodeName()
            : base(
                OutputName,
                NoItems,
                GetItems,
                s => s.Ext_ExtractPickleKey(),
                new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectByNodeName(
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