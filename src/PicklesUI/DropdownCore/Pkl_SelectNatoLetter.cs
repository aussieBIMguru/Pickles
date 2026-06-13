using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectNatoLetter")]
    [NodeCategory("Pickles.Pkl_Data.Pkl_String")]
    [NodeDescription("Select a letter of the NATO phonetic alphabet.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectNatoLetter : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No letters available.";
        private const string OutputName = "letter";

        private static readonly Dictionary<string, string> Nato = new()
        {
            ["A"] = "Alpha",
            ["B"] = "Bravo",
            ["C"] = "Charlie",
            ["D"] = "Delta",
            ["E"] = "Echo",
            ["F"] = "Foxtrot",
            ["G"] = "Golf",
            ["H"] = "Hotel",
            ["I"] = "India",
            ["J"] = "Juliet",
            ["K"] = "Kilo",
            ["L"] = "Lima",
            ["M"] = "Mike",
            ["N"] = "November",
            ["O"] = "Oscar",
            ["P"] = "Papa",
            ["Q"] = "Quebec",
            ["R"] = "Romeo",
            ["S"] = "Sierra",
            ["T"] = "Tango",
            ["U"] = "Uniform",
            ["V"] = "Victor",
            ["W"] = "Whiskey",
            ["X"] = "X-ray",
            ["Y"] = "Yankee",
            ["Z"] = "Zulu"
        };

        private static IEnumerable<string> GetItems(NodeModel node) => Nato.Keys;

        public Pkl_SelectNatoLetter() : base(
            OutputName, NoItems,
            GetItems,
            x => $"{Nato[x]}",
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectNatoLetter(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            x => $"{Nato[x]}",
            new StringOutputStrategy(),
            inPorts, outPorts)
        { }
    }
}