using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectSchedule")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_ViewSchedule")]
    [NodeDescription("Select a Schedule from the current document.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectSchedule : DropDownFactoryBase<DB.ViewSchedule>
    {
        private const string NoItems = "No Schedules available in project.";
        private const string OutputName = "viewSchedule";

        private static IEnumerable<DB.ViewSchedule> GetItems(NodeModel node)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (doc == null) return [];

            return new DB.FilteredElementCollector(doc)
                .OfClass(typeof(DB.ViewSchedule))
                .Cast<DB.ViewSchedule>()
                .Where(s => !s.Name.Contains("<Revision Schedule>"));
        }

        public Pkl_SelectSchedule() : base(
            OutputName, NoItems,
            GetItems,
            x => x.Name,
            new ElementOutputStrategy<DB.ViewSchedule>())
        { }

        [JsonConstructor]
        public Pkl_SelectSchedule(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            x => x.Name,
            new ElementOutputStrategy<DB.ViewSchedule>(),
            inPorts, outPorts)
        { }
    }
}