using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using System.Drawing.Printing;

namespace PicklesUI
{
    [NodeName("Pkl_SelectPrinter")]
    [NodeCategory("Pickles.Pkl_Application.Pkl_System")]
    [NodeDescription("Select from the printers installed on this computer.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectPrinter : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No printers found.";
        private const string OutputName = "printerName";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                yield return printer;
            }
        }

        public Pkl_SelectPrinter() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectPrinter(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy(),
            inPorts,
            outPorts)
        { }
    }
}