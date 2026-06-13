using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;
using Autodesk.Revit.DB;

namespace PicklesUI
{
    [NodeName("Pkl_SelectSheetCollection")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Selection")]
    [NodeDescription("Select a SheetCollection from the current document (all sheets returns the ProjectBrowser object).")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectSheetCollection : DropDownFactoryBase<DB.Element>
    {
        private const string NoItems = "No Sheets available in project.";
        private const string OutputName = "sheetCollection";

        public static string CollectionName(DB.Element element, DB.Document? doc = null)
        {
            doc ??= DocumentManager.Instance.CurrentDBDocument;

            if (element is null)
            {
                return "_All Sheets";
            }
            else if (element is SheetCollection)
            {
                return element.Name;
            }
            else
            {
                return "_Not in a Collection";
            }
        }

        private static IEnumerable<DB.Element> GetItems(NodeModel node)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return Enumerable.Empty<DB.Element>();

            DB.View? projectBrowser = new FilteredElementCollector(doc)
                .OfClass(typeof(DB.View))
                .Cast<DB.View>()
                .FirstOrDefault(v => v.ViewType == DB.ViewType.ProjectBrowser);

            var output = new List<DB.Element>()
            {
                null, projectBrowser
            };

            IList<DB.Element> collections = new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.SheetCollection))
                .OrderBy(e => e.Name)
                .ToList();

            output.AddRange(collections);

            return output;
        }

        public Pkl_SelectSheetCollection() : base(
            OutputName, NoItems,
            GetItems,
            s => CollectionName(s),
            new ElementOutputStrategy<DB.Element>())
        { }

        [JsonConstructor]
        public Pkl_SelectSheetCollection(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            s => CollectionName(s),
            new ElementOutputStrategy<DB.Element>(),
            inPorts, outPorts)
        { }
    }
}