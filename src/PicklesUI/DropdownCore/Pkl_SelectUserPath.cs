using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using System.IO;

namespace PicklesUI
{
    [NodeName("Pkl_SelectUserPath")]
    [NodeCategory("Pickles.Pkl_Application.Pkl_System")]
    [NodeDescription("Select from a list of common user directory paths.")]
    [IsDesignScriptCompatible]
    public class Pkl_SelectUserPath : DropDownFactoryBaseCore<string>
    {
        private const string NoItems = "No paths available.";
        private const string OutputName = "directoryPath";

        private static readonly Dictionary<string, string> FolderPaths = new()
        {
            { Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Desktop" },
            { Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Documents" },
            { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), "Downloads" },
            { Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Pictures" },
            { Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Music" },
            { Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Videos" },
            { Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppData (Roaming)" },
            { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppData (Local)" },
            { Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ProgramData" },
            { Path.GetTempPath(), "Temporary files" },
            { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "User Profile" }
        };

        private static IEnumerable<string> GetItems(NodeModel node)
        {
            return FolderPaths.Keys.OrderBy(p => p).ToArray();
        }

        public Pkl_SelectUserPath() : base(
            OutputName, NoItems,
            GetItems,
            x => FolderPaths[x],
            new StringOutputStrategy())
        { }

        [JsonConstructor]
        public Pkl_SelectUserPath(
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) : base(
            OutputName, NoItems,
            GetItems,
            x => FolderPaths[x],
            new StringOutputStrategy(),
            inPorts, outPorts)
        { }
    }
}