using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace PicklesUI
{
    [NodeName("Pkl_SelectFileFilter")]
    [NodeCategory("Pickles.Pkl_Application.Pkl_File")]
    [NodeDescription("Select from a list of common file filter strings.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectFileFilter : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No filters available.";
        private const string OutputName = "fileFilter";

        private static readonly Dictionary<string, string?> FileFilters = new()
        {
            ["Excel Files (*.xlsx;*.xls;*.xlsm)|*.xlsx;*.xls;*.xlsm"] = "Excel",
            ["CSV Files (*.csv)|*.csv"] = "CSV",
            ["TSV Files (*.tsv)|*.tsv"] = "TSV",

            ["Text Files (*.txt)|*.txt"] = "Text",
            ["JSON Files (*.json)|*.json"] = "JSON",
            ["PDF Files (*.pdf)|*.pdf"] = "PDF",
            ["Zip files (*.zip)|*.zip"] = "ZIP",

            ["Revit Family Files (*.rfa)|*.rfa"] = "Revit Family",
            ["Revit Project Files (*.rvt)|*.rvt"] = "Revit Project",
            ["Revit Template Files (*.rte)|*.rte"] = "Revit Template",

            ["AutoCAD Drawing (*.dwg)|*.dwg"] = "DWG",

            ["XML Files (*.xml)|*.xml"] = "XML",
            ["YAML Files (*.yml;*.yaml)|*.yml;*.yaml"] = "YAML",
            ["SQLite Database (*.sqlite;*.db)|*.sqlite;*.db"] = "SQLite",

            ["Dynamo Graph (*.dyn)|*.dyn"] = "Dynamo Graph",

            ["IFC Files (*.ifc)|*.ifc"] = "IFC",
            ["STEP Files (*.step;*.stp)|*.step;*.stp"] = "STEP",
            ["OBJ Files (*.obj)|*.obj"] = "OBJ",
            ["FBX Files (*.fbx)|*.fbx"] = "FBX",

            ["CAD Files (*.dwg;*.dxf)|*.dwg;*.dxf"] = "CAD Files",
            ["BIM Files (*.rvt;*.rfa;*.ifc)|*.rvt;*.rfa;*.ifc"] = "BIM Files"
        };

        private static IEnumerable<string> GetItems(NodeModel node) => FileFilters.Keys;

        public Pkl_SelectFileFilter() : base(
            OutputName, NoItems,
            GetItems,
            x => $"{FileFilters[x]}",
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectFileFilter(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            x => $"{FileFilters[x]}",
            new StringOutputStrategy(),
            inPorts, outPorts)
        { }
    }
}