using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace PicklesUI
{
    /// <summary>
    /// Select from pickled keys.
    /// </summary>
    [NodeName("Pkl_SelectByKey2")]
    [NodeCategory("Pickles.Pkl_Data.Pkl_Pickling")]
    [NodeDescription("Select from a list of pickled objects, which can then be unpickled in your graph.\n\n" +
        "This node draws upon the second pickled key in the graph.\n\n" +
        "NOTE: Works in canvas and player.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectByKey2 : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No pickles found.";
        private const string OutputName = "pickles";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            var keyNames = GraphStorage.GetKeys();
            var keyName = keyNames.Length > 1 ? keyNames[1] : keyNames.LastOrDefault() ?? "";

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

        public Pkl_SelectByKey2()
            : base(
                OutputName,
                NoItems,
                GetItems,
                s => s.Ext_ExtractPickleKey(),
                new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectByKey2(
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