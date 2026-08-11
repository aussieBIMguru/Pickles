using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectFamily")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Family")]
    [NodeDescription("Select from the available Families.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectFamily : DropDownFactoryBaseCore<DB.Family>
    {
        private const string NoItems = "No families found.";
        private const string OutputName = "family";

        private static IEnumerable<DB.Family> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return [];

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.Family))
                .Cast<DB.Family>()
                .Where(f => !f.IsInPlace)
                .ToList();
        }

        private static string FamilyKey(DB.Family family)
        {
            return $"{family.FamilyCategory.Name}: {family.Name}";
        }

        public Pkl_SelectFamily() : base(
            OutputName,
            NoItems,
            GetItems,
            x => FamilyKey(x),
            new ElementOutputStrategy<DB.Family>())
        { }

        [JsonConstructor]
        public Pkl_SelectFamily(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName,
            NoItems,
            GetItems,
            x => FamilyKey(x),
            new ElementOutputStrategy<DB.Family>(),
            inPorts, outPorts)
        { }
    }
}