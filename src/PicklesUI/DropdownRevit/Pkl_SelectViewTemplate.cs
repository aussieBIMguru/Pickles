using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectViewTemplate")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Selection")]
    [NodeDescription("Select a view template from the current document.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectViewTemplate : DropDownFactoryBase<DB.View>
    {
        private const string NoItems = "No view templates available in project.";
        private const string OutputName = "viewTemplate";

        private static IEnumerable<DB.View> GetItems()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return Enumerable.Empty<DB.View>();

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.View))
                .Cast<DB.View>()
                .Where(v => v.IsTemplate);
        }

        public Pkl_SelectViewTemplate() : base(
            OutputName, NoItems,
            GetItems,
            x => $"{x.ViewType}: {x.Name}",
            new ElementOutputStrategy<DB.View>())
        { }

        [JsonConstructor]
        public Pkl_SelectViewTemplate(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            x => $"{x.ViewType}: {x.Name}",
            new ElementOutputStrategy<DB.View>(),
            inPorts, outPorts)
        { }
    }
}