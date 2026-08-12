using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectCadExportOption")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Export")]
    [NodeDescription("Select from the available BaseExportOptions.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectCadExportOption : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No settings found.";
        private const string OutputName = "optionName";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            return DB.BaseExportOptions.GetPredefinedSetupNames(doc);
        }

        public Pkl_SelectCadExportOption() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectCadExportOption(
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