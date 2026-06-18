using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using RevitServices.Persistence;

namespace PicklesUI
{
    [NodeName("Pkl_SelectDocument")]
    [NodeCategory("Pickles.Pkl_Revit.Pkl_Application")]
    [NodeDescription("Select from the available Documents. You must convert this to a Document, as it just returns the name.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectDocument : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No documents found.";
        private const string OutputName = "documentTitle";

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            var currentDoc = DocumentManager.Instance.CurrentDBDocument;
            List<string> docTitles = new();

            foreach (DB.Document document in currentDoc.Application.Documents)
            {
                if (document.IsLinked || document.Title == currentDoc.Title)
                {
                    docTitles.Add(document.Title);
                }
            }

            return docTitles;
        }

        public Pkl_SelectDocument() : base(
            OutputName,
            NoItems,
            GetItems,
            x => x,
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectDocument(
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