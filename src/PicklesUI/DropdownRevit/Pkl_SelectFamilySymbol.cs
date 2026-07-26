using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectFamilySymbol")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_FamilySymbol")]
    [NodeDescription("Select from the available FamilySymbols.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectFamilySymbol : DropDownFactoryBaseCore<DB.FamilySymbol>
    {
        private const string NoItems = "No family types found.";
        private const string OutputName = "familyType";

        private static IEnumerable<DB.FamilySymbol> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return Enumerable.Empty<DB.FamilySymbol>();

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.FamilySymbol))
                .Cast<DB.FamilySymbol>()
                .Where(f => !f.Family.IsInPlace)
                .ToList();
        }

        private static string FamilySymbolKey(DB.FamilySymbol familySymbol)
        {
            DB.Family family = familySymbol.Family;
            return $"{family.FamilyCategory.Name}: {family.Name}: {familySymbol.Name}";
        }

        public Pkl_SelectFamilySymbol() : base(
            OutputName,
            NoItems,
            GetItems,
            x => FamilySymbolKey(x),
            new ElementOutputStrategy<DB.FamilySymbol>())
        { }

        [JsonConstructor]
        public Pkl_SelectFamilySymbol(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName,
            NoItems,
            GetItems,
            x => FamilySymbolKey(x),
            new ElementOutputStrategy<DB.FamilySymbol>(),
            inPorts, outPorts)
        { }
    }
}