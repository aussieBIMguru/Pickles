using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectViewportType")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Viewport")]
    [NodeDescription("Select a ViewportType from the current document.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectViewportType : DropDownFactoryBase<DB.Element>
    {
        private const string NoItems = "No ViewportTypes available in project.";
        private const string OutputName = "viewportType";

        private static IEnumerable<DB.Element> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return [];

            // Construct the collector rule
            DB.FilterRule? collectorRule = DB.ParameterFilterRuleFactory.CreateEqualsRule(
                new DB.ElementId(DB.BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM),
                "Viewport");
            var parameterFilter = new DB.ElementParameterFilter(collectorRule);

            // Return the elements
            return new DB.FilteredElementCollector(doc)
                .WhereElementIsElementType()
                .WherePasses(parameterFilter)
                .ToElements();
        }

        public Pkl_SelectViewportType() : base(
            OutputName, NoItems,
            GetItems,
            e => e.Name,
            new ElementOutputStrategy<DB.Element>())
        { }

        [JsonConstructor]
        public Pkl_SelectViewportType(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            e => e.Name,
            new ElementOutputStrategy<DB.Element>(),
            inPorts, outPorts)
        { }
    }
}