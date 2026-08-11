using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectTitleBlockType")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Sheet")]
    [NodeDescription("Select from the available TitleBlock types.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectTitleBlockType : DropDownFactoryBaseCore<DB.FamilySymbol>
    {
        private const string NoItems = "No TitleBlock types found.";
        private const string OutputName = "titleBlockType";

        private static IEnumerable<DB.FamilySymbol> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return [];

            int titleBlocksId = (int)DB.BuiltInCategory.OST_TitleBlocks;

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.FamilySymbol))
                .Cast<DB.FamilySymbol>()
                .Where(f => f.Category?.Id.Value == titleBlocksId);
        }

        private static string TitleBlockKey(DB.FamilySymbol familySymbol)
        {
            return $"{familySymbol.Family.Name}: {familySymbol.Name}";
        }

        public Pkl_SelectTitleBlockType() : base(
            OutputName,
            NoItems,
            GetItems,
            x => TitleBlockKey(x),
            new ElementOutputStrategy<DB.FamilySymbol>())
        { }

        [JsonConstructor]
        public Pkl_SelectTitleBlockType(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName,
            NoItems,
            GetItems,
            x => TitleBlockKey(x),
            new ElementOutputStrategy<DB.FamilySymbol>(),
            inPorts, outPorts)
        { }
    }
}