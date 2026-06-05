using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectSheet")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Selection")]
    [NodeDescription("Select a Sheet from the current document.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectSheet : DropDownFactoryBase<DB.ViewSheet>
    {
        private const string NoItems = "No Sheets available in project.";
        private const string OutputName = "sheet";

        private static IEnumerable<DB.ViewSheet> GetItems()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return Enumerable.Empty<DB.ViewSheet>();

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.ViewSheet))
                .Cast<DB.ViewSheet>();
        }


        public Pkl_SelectSheet() : base(
            OutputName, NoItems,
            GetItems,
            s => $"{s.SheetNumber}: ({s.Name})",
            new ElementOutputStrategy<DB.ViewSheet>())
        { }

        [JsonConstructor]
        public Pkl_SelectSheet(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            s => $"{s.SheetNumber}: ({s.Name})",
            new ElementOutputStrategy<DB.ViewSheet>(),
            inPorts, outPorts)
        { }
    }
}