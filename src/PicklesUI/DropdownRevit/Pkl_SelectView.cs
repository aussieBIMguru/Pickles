using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectView")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_View")]
    [NodeDescription("Select a view from the current document.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectView : DropDownFactoryBase<DB.View>
    {
        private const string NoItems = "No views available in project.";
        private const string OutputName = "view";

        private static IEnumerable<DB.View> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return Enumerable.Empty<DB.View>();

            // Ignore types
            var ignoreTypes = new HashSet<DB.ViewType>()
            {
                // Browser/Internal
                DB.ViewType.Internal,
                DB.ViewType.ProjectBrowser,
                DB.ViewType.SystemBrowser,
                DB.ViewType.Undefined,
                
                // Documentation
                DB.ViewType.Legend,
                DB.ViewType.DrawingSheet,
                DB.ViewType.Schedule,
                DB.ViewType.ColumnSchedule,
                DB.ViewType.PanelSchedule,
                
                // Reports
                DB.ViewType.Report,
                DB.ViewType.CostReport,
                DB.ViewType.LoadsReport,
                DB.ViewType.PressureLossReport,
                DB.ViewType.SystemsAnalysisReport,
            };

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.View))
                .Cast<DB.View>()
                .Where(v => !v.IsTemplate && !ignoreTypes.Contains(v.ViewType));
        }

        public Pkl_SelectView() : base(
            OutputName, NoItems,
            GetItems,
            x => $"{x.ViewType}: {x.Name}",
            new ElementOutputStrategy<DB.View>())
        { }

        [JsonConstructor]
        public Pkl_SelectView(
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